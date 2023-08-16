using NadekoUpdater.Services.Abstractions;
using System.Runtime.InteropServices;

namespace NadekoUpdater.Services;

/// <summary>
/// Service that checks, downloads, installs, and updates yt-dlp.
/// </summary>
/// <remarks>Source: https://github.com/yt-dlp/yt-dlp/releases/latest</remarks>
public sealed class YtdlpResolver : IYtdlpResolver
{
    private const string _ytdlpProcessName = "yt-dlp";
    private readonly IHttpClientFactory _httpClientFactory;

    /// <inheritdoc />
    public string DependencyName { get; } = "Yt-dlp";

    /// <inheritdoc />
    public string FileName { get; } = GetFileName();

    /// <summary>
    /// Creates a service that checks, downloads, installs, and updates yt-dlp.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    public YtdlpResolver(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    /// <inheritdoc />
    public async ValueTask<(string? OldVersion, string? NewVersion)> InstallOrUpdateAsync(string dependenciesUri, CancellationToken cToken = default)
    {
        var currentVersion = await GetCurrentVersionAsync(cToken);
        var newVersion = await GetLatestVersionAsync(cToken);

        // Update
        if (currentVersion is not null)
        {
            // If the versions are the same, exit.
            if (currentVersion == newVersion)
                return (currentVersion, null);

            using var ytdlp = Utilities.StartProcess(_ytdlpProcessName, "-U");
            await ytdlp.WaitForExitAsync(cToken);

            return (currentVersion, newVersion);
        }

        // Install
        Directory.CreateDirectory(dependenciesUri);

        using var http = _httpClientFactory.CreateClient();
        using var downloadStream = await http.GetStreamAsync($"https://github.com/yt-dlp/yt-dlp/releases/download/{newVersion}/{FileName}", cToken);
        using var fileStream = new FileStream(Path.Combine(dependenciesUri, FileName), FileMode.Create);

        await downloadStream.CopyToAsync(fileStream, cToken);

        // Update environment variable
        Utilities.AddPathToPATHEnvar(dependenciesUri);

        return (null, newVersion);
    }

    /// <inheritdoc />
    public async ValueTask<bool?> CanUpdateAsync(CancellationToken cToken = default)
    {
        var currentVer = await GetCurrentVersionAsync(cToken);

        if (currentVer is null)
            return null;

        var latestVer = await GetLatestVersionAsync(cToken);

        return !latestVer.Equals(currentVer, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public async ValueTask<string?> GetCurrentVersionAsync(CancellationToken cToken = default)
    {
        if (!await Utilities.ProgramExistsAsync(_ytdlpProcessName, cToken))
            return null;

        using var ytdlp = Utilities.StartProcess(_ytdlpProcessName, "--version");

        return (await ytdlp.StandardOutput.ReadToEndAsync(cToken)).Trim();
    }

    /// <inheritdoc />
    public async ValueTask<string> GetLatestVersionAsync(CancellationToken cToken = default)
    {
        using var http = _httpClientFactory.CreateClient(AppStatics.NoRedirectClient);

        var response = await http.GetAsync("https://github.com/yt-dlp/yt-dlp/releases/latest", cToken);

        var lastSlashIndex = response.Headers.Location?.OriginalString.LastIndexOf('/')
            ?? throw new InvalidOperationException("Failed to get the latest yt-dlp version.");

        return response.Headers.Location.OriginalString[(lastSlashIndex + 1)..];
    }

    /// <summary>
    /// Gets the name of the yt-dlp binary file to be downloaded.
    /// </summary>
    /// <returns>The name of the yt-dlp binary file.</returns>
    /// <exception cref="NotSupportedException">Occurs when this method is executed in an unsupported platform.</exception>
    private static string GetFileName()
    {
        return RuntimeInformation.OSArchitecture switch
        {
            // Windows
            Architecture.X86 when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "yt-dlp_x86.exe",
            Architecture.X64 when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "yt-dlp.exe",

            // Linux
            Architecture.X86 or Architecture.X64 when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "yt-dlp_linux",
            Architecture.Arm64 when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "yt-dlp_linux_aarch64",

            // MacOS
            Architecture.X86 or Architecture.X64 when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => "yt-dlp_macos_legacy",
            Architecture.Arm64 when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => "yt-dlp_macos",
            _ => throw new NotSupportedException($"Architecture of type {RuntimeInformation.OSArchitecture} is not supported by yt-dlp on this OS.")
        };
    }
}