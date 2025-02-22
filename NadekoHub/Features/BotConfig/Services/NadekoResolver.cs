using Kotz.Utilities;
using Microsoft.Extensions.Caching.Memory;
using NadekoHub.Features.AppConfig.Services.Abstractions;
using NadekoHub.Features.BotConfig.Models.Api.Gitlab;
using NadekoHub.Features.BotConfig.Services.Abstractions;
using SingleFileExtractor.Core;
using System.Formats.Tar;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NadekoHub.Features.BotConfig.Services;

/// <summary>
/// Service that checks, downloads, installs, and updates a NadekoBot instance.
/// </summary>
/// <remarks>Source: https://gitlab.com/Kwoth/nadekobot/-/releases/permalink/latest</remarks>
public sealed partial class NadekoResolver : IBotResolver
{
    private const string _cachedCurrentVersionKey = "currentVersion:NadekoBot";
    private const string _gitlabReleasesEndpointUrl = "https://gitlab.com/api/v4/projects/9321079/releases/permalink/latest";
    private const string _gitlabReleasesRepoUrl = "https://gitlab.com/Kwoth/nadekobot/-/releases/permalink/latest";
    private static readonly HashSet<Guid> _updateIdOngoing = [];
    private static readonly string _tempDirectory = Path.GetTempPath();
    private static readonly Regex _unzippedDirRegex = GenerateUnzipedDirRegex();
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly IAppConfigManager _appConfigManager;

    /// <inheritdoc/>
    public string DependencyName { get; } = "NadekoBot";

    /// <inheritdoc/>
    public string FileName { get; } = (OperatingSystem.IsWindows()) ? "NadekoBot.exe" : "NadekoBot";

    /// <inheritdoc/>
    public bool IsUpdateInProgress
        => _updateIdOngoing.Contains(Id);

    /// <inheritdoc/>
    public Guid Id { get; }

    /// <inheritdoc/>
    public string BotName { get; }

    /// <summary>
    /// Creates a service that checks, downloads, installs, and updates a NadekoBot instance.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="memoryCache">The memory cache.</param>
    /// <param name="appConfigManager">The application's settings.</param>
    /// <param name="botId">The Id of the bot.</param>
    public NadekoResolver(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache, IAppConfigManager appConfigManager, Guid botId)
    {
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _appConfigManager = appConfigManager;
        Id = botId;
        BotName = _appConfigManager.AppConfig.BotEntries[Id].Name;
    }

    /// <inheritdoc/>
    public async ValueTask<bool?> CanUpdateAsync(CancellationToken cToken = default)
    {
        var currentVersion = await GetCurrentVersionAsync(cToken);

        if (currentVersion is null)
            return null;

        var latestVersion = await GetLatestVersionAsync(cToken);

        if (Version.Parse(latestVersion) <= Version.Parse(currentVersion))
            return false;

        var http = _httpClientFactory.CreateClient();

        return await http.IsUrlValidAsync(
            await GetDownloadUrlAsync(latestVersion, cToken),
            cToken
        );
    }

    /// <inheritdoc/>
    public async ValueTask<string?> CreateBackupAsync()
    {
        var botInstance = _appConfigManager.AppConfig.BotEntries[Id];

        if (!Directory.Exists(botInstance.InstanceDirectoryUri))
            return null;

        Directory.CreateDirectory(_appConfigManager.AppConfig.BotsBackupDirectoryUri);

        var now = DateTimeOffset.Now;
        var date = new DateOnly(now.Year, now.Month, now.Day).ToShortDateString().Replace('/', '-');
        var backupZipName = $"{botInstance.Name}_v{botInstance.Version}_{date}-{now.ToUnixTimeMilliseconds()}.zip";
        var destinationUri = Path.Join(_appConfigManager.AppConfig.BotsBackupDirectoryUri, backupZipName);

        // ZipFile does not provide asynchronous implementations, so we have to schedule its
        // execution to be run in the thread-pool due to how long it takes to finish execution.
        await Task.Run(() => ZipFile.CreateFromDirectory(botInstance.InstanceDirectoryUri, destinationUri, CompressionLevel.SmallestSize, true));

        return destinationUri;
    }

    /// <inheritdoc/>
    public async ValueTask<string?> GetCurrentVersionAsync(CancellationToken cToken = default)
    {
        var botEntry = _appConfigManager.AppConfig.BotEntries[Id];
        var executableUri = Path.Join(botEntry.InstanceDirectoryUri, FileName);

        if (!File.Exists(executableUri))
        {
            await _appConfigManager.UpdateBotEntryAsync(Id, x => x with { Version = null }, cToken);
            return null;
        }

        return (string.IsNullOrWhiteSpace(botEntry.Version))
            ? await GetBotVersionFromAssemblyAsync(executableUri, cToken)
            : botEntry.Version;
    }

    /// <inheritdoc/>
    public async ValueTask<string> GetLatestVersionAsync(CancellationToken cToken = default)
    {
        try
        {
            return (await GetLatestVersionFromApiAsync(cToken)).Tag;
        }
        catch (InvalidOperationException)
        {
            return await GetLatestVersionFromUrlAsync(cToken);
        }
    }

    /// <inheritdoc/>
    public async ValueTask<(string? OldVersion, string? NewVersion)> InstallOrUpdateAsync(string installationUri, CancellationToken cToken = default)
    {
        if (IsUpdateInProgress)
            return (null, null);

        _updateIdOngoing.Add(Id);

        var currentVersion = await GetCurrentVersionAsync(cToken);
        var latestVersion = await GetLatestVersionAsync(cToken);

        // Already up-to-date, quit
        if (currentVersion is not null && Version.Parse(latestVersion) <= Version.Parse(currentVersion))
        {
            _updateIdOngoing.Remove(Id);
            return (currentVersion, null);
        }

        var backupFileUri = await CreateBackupAsync();

        if (currentVersion is not null)
            Directory.Delete(installationUri, true);

        // Install
        Directory.CreateDirectory(_appConfigManager.AppConfig.BotsDirectoryUri);

        var http = _httpClientFactory.CreateClient();
        var downloadFileName = GetDownloadFileName(latestVersion);
        var botTempLocation = Path.Join(_tempDirectory, "nadekobot-" + _unzippedDirRegex.Match(downloadFileName).Groups[1].Value);
        var zipTempLocation = Path.Join(_tempDirectory, downloadFileName);

        try
        {
            await using var downloadStream = await http.GetStreamAsync(
                await GetDownloadUrlAsync(latestVersion, cToken),
                cToken
            );

            // Move the bot root directory
            if (Environment.OSVersion.Platform is not PlatformID.Unix)
                await InstallToWindowsAsync(downloadStream, installationUri, zipTempLocation, botTempLocation, cToken);
            else
                await InstallToUnixAsync(downloadStream, installationUri, botTempLocation, cToken);

            // Reapply bot settings
            if (File.Exists(backupFileUri))
                await ReaplyBotSettingsAsync(installationUri, backupFileUri, cToken);

            // Update settings
            await _appConfigManager.UpdateBotEntryAsync(Id, x => x with { Version = latestVersion }, cToken);

            // Create creds.yml if it doesn't exist
            // Old versions have creds.yml and example in ./
            // New versions have creds.yml and example in ./data/
            // If downloaded bot has creds example in ./, create creds.yml to ./, else create to ./data/
            var credsExampleUri = Directory.EnumerateFiles(installationUri, "creds_example.yml", SearchOption.AllDirectories)
                .Last();
            
            var credsUri = Path.Join(Directory.GetParent(credsExampleUri)?.FullName ?? Path.Join(installationUri, "data"), "creds.yml");

            if (!File.Exists(credsUri))
                File.Copy(credsExampleUri, credsUri);

            return (currentVersion, latestVersion);
        }
        finally
        {
            _updateIdOngoing.Remove(Id);

            // Cleanup
            KotzUtilities.TryDeleteFile(zipTempLocation);
            KotzUtilities.TryDeleteDirectory(botTempLocation);
        }
    }

    /// <summary>
    /// Installs the Nadeko instance on a Unix system.
    /// </summary>
    /// <param name="downloadStream">The stream of data downloaded from the source.</param>
    /// <param name="installationUri">The absolute path to the directory the bot got installed to.</param>
    /// <param name="botTempLocation">The absolute path to the temporary directory the bot is extracted to.</param>
    /// <param name="cToken">The cancellation token.</param>
    private async ValueTask InstallToUnixAsync(Stream downloadStream, string installationUri, string botTempLocation, CancellationToken cToken = default)
    {
        // Extract the tar ball
        await TarFile.ExtractToDirectoryAsync(downloadStream, _tempDirectory, true, cToken);

        KotzUtilities.TryMoveDirectory(botTempLocation, installationUri);

        // Set executable permission
        using var chmod = KotzUtilities.StartProcess("chmod", ["+x", Path.Join(installationUri, FileName)]);
        await chmod.WaitForExitAsync(cToken);
    }

    /// <summary>
    /// Installs the Nadeko instance on a non-Unix system.
    /// </summary>
    /// <param name="downloadStream">The stream of data downloaded from the source.</param>
    /// <param name="installationUri">The absolute path to the directory the bot got installed to.</param>
    /// <param name="zipTempLocation">The absolute path to the zip file the bot is initially on.</param>
    /// <param name="botTempLocation">The absolute path to the temporary directory the bot is extracted to.</param>
    /// <param name="cToken">The cancellation token.</param>
    private static async ValueTask InstallToWindowsAsync(Stream downloadStream, string installationUri, string zipTempLocation, string botTempLocation, CancellationToken cToken = default)
    {
        // Save the zip file
        await using (var fileStream = new FileStream(zipTempLocation, FileMode.Create))
            await downloadStream.CopyToAsync(fileStream, cToken);

        // Extract the zip file
        await Task.Run(() => ZipFile.ExtractToDirectory(zipTempLocation, _tempDirectory), cToken);

        // Move the bot root directory while renaming it
        Directory.Move(botTempLocation, installationUri);
    }

    /// <summary>
    /// Reaplies the bot settings and user-defined data from the specified backup.
    /// </summary>
    /// <param name="installationUri">The absolute path to the directory the bot got installed to.</param>
    /// <param name="backupFileUri">The absolute path to the backup zip file.</param>
    /// <param name="cToken">The cancellation token.</param>
    private static async ValueTask ReaplyBotSettingsAsync(string installationUri, string backupFileUri, CancellationToken cToken = default)
    {
        using var zipFile = ZipFile.OpenRead(backupFileUri);
        var zippedFiles = zipFile.Entries
            .Where(x =>
                x.Name is "creds.yml"   // Restore creds.yml and everything in the "data" folder, but not the stuff in the "strings" folder.
                || (!string.IsNullOrWhiteSpace(x.Name) && x.FullName.Contains("data/") && !x.FullName.Contains("strings/"))
            );

        foreach (var zippedFile in zippedFiles)
        {
            var fileDestinationPath = zippedFile.FullName.Split('/')
                .Prepend(Directory.GetParent(installationUri)?.FullName ?? string.Empty)
                .ToArray();

            // Old versions have creds.yml and example in ./
            // New versions have creds.yml and example in ./data/
            // If downloaded bot has creds example in the root, restore creds.yml to root, else restore to ./data/
            if (zippedFile.Name is "creds.yml" && !File.Exists(Path.Join(installationUri, "creds_example.yml")))
                await RestoreFileAsync(zippedFile, Path.Join(installationUri, "data", zippedFile.Name), cToken);
            else
                await RestoreFileAsync(zippedFile, Path.Join(fileDestinationPath), cToken);
        }
    }

    /// <summary>
    /// Extracts the specified <paramref name="zippedFile"/> to the <paramref name="destinationPath"/>.
    /// </summary>
    /// <param name="zippedFile">The file to be extracted.</param>
    /// <param name="destinationPath">The final location of the extracted file.</param>
    /// <param name="cToken">The cancellation token.</param>
    private static async ValueTask RestoreFileAsync(ZipArchiveEntry zippedFile, string destinationPath, CancellationToken cToken = default)
    {
        await using var zipStream = zippedFile.Open();
        await using var fileStream = new FileStream(destinationPath, FileMode.Create);

        await zipStream.CopyToAsync(fileStream, cToken);
    }

    /// <summary>
    /// Gets the name of the file to be downloaded.
    /// </summary>
    /// <param name="version">The version of NadekoBot.</param>
    /// <returns>The name of the file to download.</returns>
    /// <exception cref="NotSupportedException">Occurs when this method is executed in an unsupported platform.</exception>
    private static string GetDownloadFileName(string version)
    {
        return version + RuntimeInformation.OSArchitecture switch
        {
            // Windows
            Architecture.X64 when OperatingSystem.IsWindows() => "-windows-x64-build.zip",
            Architecture.Arm64 when OperatingSystem.IsWindows() => "-windows-arm64-build.zip",

            // Linux
            Architecture.X64 when OperatingSystem.IsLinux() => "-linux-x64-build.tar",
            Architecture.Arm64 when OperatingSystem.IsLinux() => "-linux-arm64-build.tar",

            // MacOS
            Architecture.X64 when OperatingSystem.IsMacOS() => "-osx-x64-build.tar",
            Architecture.Arm64 when OperatingSystem.IsMacOS() => "-osx-arm64-build.tar",
            _ => throw new NotSupportedException($"Architecture of type {RuntimeInformation.OSArchitecture} is not supported by NadekoBot on this OS.")
        };
    }

    /// <summary>
    /// Gets the download url to the latest bot release.
    /// </summary>
    /// <param name="latestVersion">The latest version of the bot.</param>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns>The url to the latest bot release.</returns>
    private async ValueTask<string> GetDownloadUrlAsync(string latestVersion, CancellationToken cToken = default)
    {
        var downloadFileName = GetDownloadFileName(latestVersion);

        try
        {
            // The first release is the most recent one.
            return (await GetLatestVersionFromApiAsync(cToken)).Assets.Links
                .First(x => x.Name.Equals(downloadFileName, StringComparison.Ordinal))
                .Url;
        }
        catch (InvalidOperationException)
        {
            return $"https://gitlab.com/api/v4/projects/9321079/packages/generic/NadekoBot-build/{latestVersion}/{downloadFileName}";
        }
    }

    /// <summary>
    /// Gets the latest bot version from the Gitlab latest release URL.
    /// </summary>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns>The latest version of the bot.</returns>
    /// <exception cref="InvalidOperationException">Occurs when parsing of the response fails.</exception>
    private async ValueTask<string> GetLatestVersionFromUrlAsync(CancellationToken cToken = default)
    {
        var http = _httpClientFactory.CreateClient(AppConstants.NoRedirectClient);
        var response = await http.GetAsync(_gitlabReleasesRepoUrl, cToken);

        var lastSlashIndex = response.Headers.Location?.OriginalString.LastIndexOf('/')
            ?? throw new InvalidOperationException("Failed to get the latest NadekoBot version.");

        return response.Headers.Location.OriginalString[(lastSlashIndex + 1)..];
    }

    /// <summary>
    /// Gets the latest bot version from the Gitlab API.
    /// </summary>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns>The latest version of the bot.</returns>
    /// <exception cref="InvalidOperationException">Occurs when the API call fails.</exception>
    private async ValueTask<GitlabRelease> GetLatestVersionFromApiAsync(CancellationToken cToken = default)
    {
        if (_memoryCache.TryGetValue(_cachedCurrentVersionKey, out var cachedObject) && cachedObject is GitlabRelease cachedResponse)
            return cachedResponse;

        var http = _httpClientFactory.CreateClient();
        var httpResponse = await http.GetAsync(_gitlabReleasesEndpointUrl, cToken);

        if (!httpResponse.IsSuccessStatusCode)
            throw new InvalidOperationException("The call to the Gitlab API failed.");

        var response = JsonSerializer.Deserialize<GitlabRelease>(await httpResponse.Content.ReadAsStringAsync(cToken))
            ?? throw new InvalidOperationException("Failed deserializing Gitlab's response.");

        _memoryCache.Set(_cachedCurrentVersionKey, response, TimeSpan.FromMinutes(1));

        return response;
    }

    /// <summary>
    /// Gets the bot version from the bot's assembly.
    /// </summary>
    /// <param name="executableUri">The path to the bot's executable file.</param>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns>The version of the bot or <see langword="null"/> if the executable file is not found.</returns>
    /// <exception cref="InvalidOperationException">Occurs when the assembly file is not found.</exception>
    private async ValueTask<string?> GetBotVersionFromAssemblyAsync(string executableUri, CancellationToken cToken)
    {
        if (!File.Exists(executableUri))
            return null;

        var directoryUri = Directory.GetParent(executableUri)?.FullName ?? Path.GetPathRoot(executableUri)!;
        var assemblyUri = Path.Join(directoryUri, "NadekoBot.dll");
        var isSingleFile = !File.Exists(assemblyUri);

        // If Nadeko is published as a single-file binary, we have to extract
        // its contents first in order to read the assembly for its version.
        if (isSingleFile)
        {
            directoryUri = Path.Join(_tempDirectory, "NadekoBotExtract_" + DateTimeOffset.Now.Ticks);
            assemblyUri = Path.Join(directoryUri, "NadekoBot.dll");
            using var executableReader = new ExecutableReader(executableUri);
            await executableReader.ExtractToDirectoryAsync(directoryUri, cToken);
        }

        try
        {
            var nadekoAssembly = Assembly.LoadFile(assemblyUri);
            var version = nadekoAssembly.GetName().Version
                ?? throw new InvalidOperationException($"Could not find version for the assembly at {assemblyUri}.");

            var currentVersion = $"{version.Major}.{version.Minor}.{version.Build}";

            await _appConfigManager.UpdateBotEntryAsync(Id, x => x with { Version = currentVersion }, cToken);

            return currentVersion;
        }
        finally
        {
            if (isSingleFile)
                KotzUtilities.TryDeleteDirectory(directoryUri);
        }
    }

    [GeneratedRegex(@"^(?:\S+\-)(\S+\-\S+)\-", RegexOptions.Compiled)]
    private static partial Regex GenerateUnzipedDirRegex();
}