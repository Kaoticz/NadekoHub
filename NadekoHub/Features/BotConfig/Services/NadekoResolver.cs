using NadekoHub.Features.AppConfig.Services.Abstractions;
using NadekoHub.Features.BotConfig.Services.Abstractions;
using System.Formats.Tar;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace NadekoHub.Services;

/// <summary>
/// Service that checks, downloads, installs, and updates a NadekoBot instance.
/// </summary>
/// <remarks>Source: https://gitlab.com/Kwoth/nadekobot/-/releases/permalink/latest</remarks>
public sealed partial class NadekoResolver : IBotResolver
{
    private static readonly HashSet<Guid> _updateIdOngoing = [];
    private static readonly string _tempDirectory = Path.GetTempPath();
    private static readonly Regex _unzipedDirRegex = GenerateUnzipedDirRegex();
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAppConfigManager _appConfigManager;

    /// <inheritdoc/>
    public string DependencyName { get; } = "NadekoBot";

    /// <inheritdoc/>
    public string FileName { get; } = (OperatingSystem.IsWindows()) ? "NadekoBot.exe" : "NadekoBot";

    /// <inheritdoc/>
    public Guid Id { get; }

    /// <inheritdoc/>
    public string BotName { get; }

    /// <summary>
    /// Creates a service that checks, downloads, installs, and updates a NadekoBot instance.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="appConfigManager">The application's settings.</param>
    /// <param name="botId">The Id of the bot.</param>
    public NadekoResolver(IHttpClientFactory httpClientFactory, IAppConfigManager appConfigManager, Guid botId)
    {
        _httpClientFactory = httpClientFactory;
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
            $"https://gitlab.com/api/v4/projects/9321079/packages/generic/NadekoBot-build/{latestVersion}/{GetDownloadFileName(latestVersion)}",
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
        var destinationUri = Path.Combine(_appConfigManager.AppConfig.BotsBackupDirectoryUri, backupZipName);

        // ZipFile does not provide asynchronous implementations, so we have to schedule its
        // execution to be run in the thread-pool due to how long it takes to finish execution.
        await Task.Run(() => ZipFile.CreateFromDirectory(botInstance.InstanceDirectoryUri, destinationUri, CompressionLevel.SmallestSize, true));

        return destinationUri;
    }

    /// <inheritdoc/>
    public async ValueTask<string?> GetCurrentVersionAsync(CancellationToken cToken = default)
    {
        var botEntry = _appConfigManager.AppConfig.BotEntries[Id];

        if (!string.IsNullOrWhiteSpace(botEntry.Version))
            return botEntry.Version;

        var assemblyUri = Path.Combine(botEntry.InstanceDirectoryUri, "NadekoBot.dll");

        if (!File.Exists(assemblyUri))
            return null;

        var nadekoAssembly = Assembly.LoadFile(assemblyUri);
        var version = nadekoAssembly.GetName().Version
            ?? throw new InvalidOperationException($"Could not find version of the assembly at {assemblyUri}.");

        var currentVersion = $"{version.Major}.{version.Minor}.{version.Build}";

        await _appConfigManager.UpdateBotEntryAsync(Id, x => x with { Version = currentVersion }, cToken);

        return currentVersion;
    }

    /// <inheritdoc/>
    public async ValueTask<string> GetLatestVersionAsync(CancellationToken cToken = default)
    {
        var http = _httpClientFactory.CreateClient(AppConstants.NoRedirectClient);

        var response = await http.GetAsync("https://gitlab.com/Kwoth/nadekobot/-/releases/permalink/latest", cToken);

        var lastSlashIndex = response.Headers.Location?.OriginalString.LastIndexOf('/')
            ?? throw new InvalidOperationException("Failed to get the latest NadekoBot version.");

        return response.Headers.Location.OriginalString[(lastSlashIndex + 1)..];
    }

    /// <inheritdoc/>
    public async ValueTask<(string? OldVersion, string? NewVersion)> InstallOrUpdateAsync(string installationUri, CancellationToken cToken = default)
    {
        if (_updateIdOngoing.Contains(Id))
            return (null, null);

        _updateIdOngoing.Add(Id);

        var currentVersion = await GetCurrentVersionAsync(cToken);
        var latestVersion = await GetLatestVersionAsync(cToken);

        // Update
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
        var botTempLocation = Path.Combine(_tempDirectory, "nadekobot-" + _unzipedDirRegex.Match(downloadFileName).Groups[1].Value);
        var zipTempLocation = Path.Combine(_tempDirectory, downloadFileName);

        try
        {
            using var downloadStream = await http.GetStreamAsync(
                $"https://gitlab.com/api/v4/projects/9321079/packages/generic/NadekoBot-build/{latestVersion}/{downloadFileName}",
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

            // Create creds.yml
            var credsUri = Path.Combine(installationUri, "creds.yml");

            if (!File.Exists(credsUri))
                File.Copy(Path.Combine(installationUri, "creds_example.yml"), credsUri);

            return (currentVersion, latestVersion);
        }
        finally
        {
            _updateIdOngoing.Remove(Id);

            // Cleanup
            Utilities.TryDeleteFile(zipTempLocation);
            Utilities.TryDeleteDirectory(botTempLocation);
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

        // Move the bot root directory with "mv" to circumvent this issue on Unix systems: https://github.com/dotnet/runtime/issues/31149
        using var moveProcess = Utilities.StartProcess("mv", $"\"{botTempLocation}\" \"{installationUri}\"");
        await moveProcess.WaitForExitAsync(cToken);

        // Set executable permission
        using var chmod = Utilities.StartProcess("chmod", $"+x \"{Path.Combine(installationUri, FileName)}\"");
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
    private async static ValueTask InstallToWindowsAsync(Stream downloadStream, string installationUri, string zipTempLocation, string botTempLocation, CancellationToken cToken = default)
    {
        // Save the zip file
        using (var fileStream = new FileStream(zipTempLocation, FileMode.Create))
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
    private async static ValueTask ReaplyBotSettingsAsync(string installationUri, string backupFileUri, CancellationToken cToken = default)
    {
        using var zipFile = ZipFile.OpenRead(backupFileUri);
        var zippedFiles = zipFile.Entries
            .Where(x =>
                x.Name is "creds.yml" or "creds_example.yml"
                || (!string.IsNullOrWhiteSpace(x.Name) && x.FullName.Contains("data/"))
            );

        foreach (var zippedFile in zippedFiles)
        {
            var fileDestinationPath = zippedFile.FullName.Split('/')
                .Prepend(Directory.GetParent(installationUri)?.FullName ?? string.Empty)
                .ToArray();

            await RestoreFileAsync(zippedFile, Path.Combine(fileDestinationPath), cToken);
        }
    }

    /// <summary>
    /// Extracts the specified <paramref name="zippedFile"/> to the <paramref name="destinationPath"/>.
    /// </summary>
    /// <param name="zippedFile">The file to be extracted.</param>
    /// <param name="destinationPath">The final location of the extracted file.</param>
    /// <param name="cToken">The cancellation token.</param>
    private async static ValueTask RestoreFileAsync(ZipArchiveEntry zippedFile, string destinationPath, CancellationToken cToken = default)
    {
        using var zipStream = zippedFile.Open();
        using var fileStream = new FileStream(destinationPath, FileMode.Create);

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

    [GeneratedRegex(@"^(?:\S+\-)(\S+\-\S+)\-", RegexOptions.Compiled)]
    private static partial Regex GenerateUnzipedDirRegex();
}