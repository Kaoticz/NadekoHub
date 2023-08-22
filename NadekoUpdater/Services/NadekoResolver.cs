using NadekoUpdater.Services.Abstractions;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace NadekoUpdater.Services;

/// <summary>
/// Service that checks, downloads, installs, and updates a NadekoBot instance
/// </summary>
/// <remarks>Source: https://gitlab.com/Kwoth/nadekobot/-/releases/permalink/latest</remarks>
public sealed partial class NadekoResolver : IBotResolver
{
    private static readonly HashSet<uint> _updateIdOngoing = new();
    private static readonly string _tempDirectory = Path.GetTempPath();
    private static readonly Regex _unzipedDirRegex = GenerateUnzipedDirRegex();
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AppConfigManager _appConfigManager;

    /// <inheritdoc/>
    public string DependencyName { get; } = "NadekoBot";

    /// <inheritdoc/>
    public string FileName { get; } = Environment.OSVersion.Platform is PlatformID.Win32NT
        ? "NadekoBot.exe"
        : "NadekoBot";

    /// <inheritdoc/>
    public uint Position { get; }

    /// <inheritdoc/>
    public string BotName { get; }

    /// <summary>
    /// Creates a service that checks, downloads, installs, and updates a NadekoBot instance
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="appConfigManager">The application's settings.</param>
    /// <param name="position">The position of the bot instance on the lateral bar.</param>
    public NadekoResolver(IHttpClientFactory httpClientFactory, AppConfigManager appConfigManager, uint position)
    {
        _httpClientFactory = httpClientFactory;
        _appConfigManager = appConfigManager;
        Position = position;
        BotName = _appConfigManager.AppConfig.BotEntries[Position].Name;
    }

    /// <inheritdoc/>
    public async ValueTask<bool?> CanUpdateAsync(CancellationToken cToken = default)
    {
        var currentVersion = await GetCurrentVersionAsync(cToken);

        if (currentVersion is null)
            return null;

        var latestVersion = await GetLatestVersionAsync(cToken);

        return !latestVersion.Equals(currentVersion, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public async ValueTask<string?> CreateBackupAsync()
    {
        var botInstance = _appConfigManager.AppConfig.BotEntries[Position];

        if (!Directory.Exists(botInstance.InstanceDirectoryUri))
            return null;

        Directory.CreateDirectory(_appConfigManager.AppConfig.BotsBackupDirectoryUri);

        var now = DateTimeOffset.Now;
        var date = new DateOnly(now.Year, now.Month, now.Day).ToShortDateString().Replace('/', '-');
        var backupZipName = $"{botInstance.Name}_{date}-{now.ToUnixTimeMilliseconds()}.zip";
        var destinationUri = Path.Combine(_appConfigManager.AppConfig.BotsBackupDirectoryUri, backupZipName);

        // ZipFile does not provide asynchronous implementations, so we have to schedule its
        // execution to be run in the thread-pool due to how long it takes to finish execution.
        await Task.Run(() => ZipFile.CreateFromDirectory(botInstance.InstanceDirectoryUri, destinationUri, CompressionLevel.SmallestSize, true));

        return destinationUri;
    }

    /// <inheritdoc/>
    public async ValueTask<string?> GetCurrentVersionAsync(CancellationToken cToken = default)
    {
        var botEntry = _appConfigManager.AppConfig.BotEntries[Position];

        if (!string.IsNullOrWhiteSpace(botEntry.Version))
            return botEntry.Version;

        var assemblyUri = Path.Combine(botEntry.InstanceDirectoryUri, "NadekoBot.dll");

        if (!File.Exists(assemblyUri))
            return null;
        var nadekoAssembly = Assembly.LoadFile(assemblyUri);
        var version = nadekoAssembly.GetName().Version
            ?? throw new InvalidOperationException($"Could not find version of the assembly at {assemblyUri}.");

        var currentVersion = $"{version.Major}.{version.Minor}.{version.Build}";

        await _appConfigManager.UpdateConfigAsync(x => x.BotEntries[Position] = x.BotEntries[Position] with { Version = currentVersion }, cToken);

        return currentVersion;
    }

    /// <inheritdoc/>
    public async ValueTask<string> GetLatestVersionAsync(CancellationToken cToken = default)
    {
        var http = _httpClientFactory.CreateClient(AppStatics.NoRedirectClient);

        var response = await http.GetAsync("https://gitlab.com/Kwoth/nadekobot/-/releases/permalink/latest", cToken);

        var lastSlashIndex = response.Headers.Location?.OriginalString.LastIndexOf('/')
            ?? throw new InvalidOperationException("Failed to get the latest NadekoBot version.");

        return response.Headers.Location.OriginalString[(lastSlashIndex + 1)..];
    }

    /// <inheritdoc/>
    public async ValueTask<(string? OldVersion, string? NewVersion)> InstallOrUpdateAsync(string installationUri, CancellationToken cToken = default)
    {
        if (_updateIdOngoing.Contains(Position))
            return (null, null);

        _updateIdOngoing.Add(Position);

        var currentVersion = await GetCurrentVersionAsync(cToken);
        var latestVersion = await GetLatestVersionAsync(cToken);

        // Update
        if (latestVersion == currentVersion)
        {
            _updateIdOngoing.Remove(Position);
            return (currentVersion, null);
        }
        
        var backupFileUri = await CreateBackupAsync();

        if (currentVersion is not null)
            Directory.Delete(installationUri, true);

        // Install
        Directory.CreateDirectory(_appConfigManager.AppConfig.BotsDirectoryUri);

        var http = _httpClientFactory.CreateClient();
        var zipFileName = GetDownloadFileName(latestVersion);
        var botTempLocation = Path.Combine(_tempDirectory, "nadekobot-" + _unzipedDirRegex.Match(zipFileName).Groups[1].Value);
        var zipTempLocation = Path.Combine(_tempDirectory, zipFileName);

        try
        {
            var zipStream = await http.GetStreamAsync(
                $"https://gitlab.com/api/v4/projects/9321079/packages/generic/NadekoBot-build/{latestVersion}/{zipFileName}",
                cToken
            );
            using (var fileStream = new FileStream(zipTempLocation, FileMode.Create))
                await zipStream.CopyToAsync(fileStream, cToken);

            // Extract the zip file
            ZipFile.ExtractToDirectory(zipTempLocation, _tempDirectory);

            // Move the bot root directory while renaming it
            Directory.Move(botTempLocation, installationUri);

            // Reapply bot settings
            if (File.Exists(backupFileUri))
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

            // Update settings
            await _appConfigManager.UpdateConfigAsync(x => x.BotEntries[Position] = x.BotEntries[Position] with { Version = latestVersion }, cToken);

            // Create creds.yml
            var credsUri = Path.Combine(installationUri, "creds.yml");

            if (!File.Exists(credsUri))
                File.Copy(Path.Combine(installationUri, "creds_example.yml"), credsUri);

            return (currentVersion, latestVersion);
        }
        finally
        {
            _updateIdOngoing.Remove(Position);

            // Cleanup
            Utilities.TryDeleteFile(zipTempLocation);
            Utilities.TryDeleteDirectory(botTempLocation);
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
            Architecture.X64 when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "-windows-x64-build.zip",
            Architecture.Arm64 when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "-windows-arm64-build.zip",

            // Linux
            Architecture.X64 when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "-linux-x64-build.zip",
            Architecture.Arm64 when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "-linux-arm64-build.zip",

            // MacOS
            Architecture.X64 when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => "-osx-x64-build.zip",
            Architecture.Arm64 when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => "-osx-arm64-build.zip",
            _ => throw new NotSupportedException($"Architecture of type {RuntimeInformation.OSArchitecture} is not supported by NadekoBot on this OS.")
        };
    }

    [GeneratedRegex(@"^(?:\S+\-)(\S+\-\S+)\-", RegexOptions.Compiled)]
    private static partial Regex GenerateUnzipedDirRegex();
}