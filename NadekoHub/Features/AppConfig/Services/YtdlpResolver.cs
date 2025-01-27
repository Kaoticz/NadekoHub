using Kotz.Utilities;
using Microsoft.Extensions.Caching.Memory;
using NadekoHub.Features.AppConfig.Services.Abstractions;
using System.Runtime.InteropServices;

namespace NadekoHub.Features.AppConfig.Services;

/// <summary>
/// Service that checks, downloads, installs, and updates yt-dlp.
/// </summary>
/// <remarks>Source: https://github.com/yt-dlp/yt-dlp/releases/latest</remarks>
public sealed class YtdlpResolver : IYtdlpResolver
{
    private const string _cachedCurrentVersionKey = "currentVersion:yt-dlp";
    private const string _ytdlpProcessName = "yt-dlp";
    private static readonly string _downloadedFileName = GetDownloadFileName();
    private bool _isUpdating;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;

    /// <inheritdoc />
    public string DependencyName { get; } = "Yt-dlp";

    /// <inheritdoc />
    public string FileName { get; } = OperatingSystem.IsWindows() ? "yt-dlp.exe" : "yt-dlp";

    /// <summary>
    /// Creates a service that checks, downloads, installs, and updates yt-dlp.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="memoryCache">The memory cache.</param>
    public YtdlpResolver(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
    {
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
    }

    /// <inheritdoc />
    public async ValueTask<bool?> CanUpdateAsync(CancellationToken cToken = default)
    {
        var currentVersion = await GetCurrentVersionAsync(cToken);

        if (currentVersion is null)
            return null;

        var latestVersion = await GetLatestVersionAsync(cToken);

        if (latestVersion.Equals(currentVersion, StringComparison.Ordinal))
            return false;

        var http = _httpClientFactory.CreateClient();

        return await http.IsUrlValidAsync($"https://github.com/yt-dlp/yt-dlp/releases/download/{latestVersion}/{_downloadedFileName}", cToken);
    }

    /// <inheritdoc />
    public async ValueTask<string?> GetCurrentVersionAsync(CancellationToken cToken = default)
    {
        // If yt-dlp is not accessible from the shell...
        if (!KotzUtilities.ProgramExists(_ytdlpProcessName))
        {
            // And doesn't exist in the dependencies folder,
            // report that yt-dlp is not installed.
            if (!File.Exists(Path.Join(AppStatics.AppDepsUri, FileName)))
                return null;

            // Else, add the dependencies directory to the PATH envar,
            // then try again.
            KotzUtilities.AddPathToPATHEnvar(AppStatics.AppDepsUri);
            return await GetCurrentVersionAsync(cToken);
        }

        // "yt-dlp --version" takes a very long time to return, so we cache the result for 90 seconds.
        if (_memoryCache.TryGetValue<string>(_cachedCurrentVersionKey, out var currentVersion) && currentVersion is not null)
            return currentVersion;

        using var ytdlp = KotzUtilities.StartProcess(_ytdlpProcessName, "--version", true);

        var currentProcessVersion = (await ytdlp.StandardOutput.ReadToEndAsync(cToken)).Trim();
        _memoryCache.Set(_cachedCurrentVersionKey, currentProcessVersion, TimeSpan.FromMinutes(1.5));

        return currentProcessVersion;
    }

    /// <inheritdoc />
    public async ValueTask<string> GetLatestVersionAsync(CancellationToken cToken = default)
    {
        var http = _httpClientFactory.CreateClient(AppConstants.GithubClient);

        var response = await http.GetAsync("https://github.com/yt-dlp/yt-dlp/releases/latest", cToken);

        var lastSlashIndex = response.Headers.Location?.OriginalString.LastIndexOf('/')
            ?? throw new InvalidOperationException("Failed to get the latest yt-dlp version.");

        return response.Headers.Location.OriginalString[(lastSlashIndex + 1)..];
    }

    /// <inheritdoc />
    public async ValueTask<(string? OldVersion, string? NewVersion)> InstallOrUpdateAsync(string installationUri, CancellationToken cToken = default)
    {
        if (_isUpdating)
            return (null, null);

        _isUpdating = true;
        var currentVersion = await GetCurrentVersionAsync(cToken);
        var newVersion = await GetLatestVersionAsync(cToken);

        _memoryCache.Remove(_cachedCurrentVersionKey);

        // Update
        if (currentVersion is not null)
        {
            // If the versions are the same, exit.
            if (currentVersion == newVersion)
            {
                _isUpdating = false;
                return (currentVersion, null);
            }

            using var ytdlp = KotzUtilities.StartProcess(_ytdlpProcessName, "-U");
            await ytdlp.WaitForExitAsync(cToken);

            _isUpdating = false;
            return (currentVersion, newVersion);
        }

        // Install
        Directory.CreateDirectory(installationUri);

        var finalFilePath = Path.Join(installationUri, FileName);
        var http = _httpClientFactory.CreateClient();
        await using var downloadStream = await http.GetStreamAsync($"https://github.com/yt-dlp/yt-dlp/releases/download/{newVersion}/{_downloadedFileName}", cToken);
        await using (var fileStream = new FileStream(finalFilePath, FileMode.Create))
            await downloadStream.CopyToAsync(fileStream, cToken);

        // Update environment variable
        KotzUtilities.AddPathToPATHEnvar(installationUri);

        // On Linux and MacOS, we need to mark the file as executable.
        if (Environment.OSVersion.Platform is PlatformID.Unix)
        {
            using var chmod = KotzUtilities.StartProcess("chmod", ["+x", finalFilePath]);
            await chmod.WaitForExitAsync(cToken);
        }

        _isUpdating = false;
        return (null, newVersion);
    }

    /// <summary>
    /// Gets the name of the yt-dlp binary file to be downloaded.
    /// </summary>
    /// <returns>The name of the yt-dlp binary file.</returns>
    /// <exception cref="NotSupportedException">Occurs when this method is executed in an unsupported platform.</exception>
    private static string GetDownloadFileName()
    {
        return RuntimeInformation.OSArchitecture switch
        {
            // Windows
            Architecture.X64 when OperatingSystem.IsWindows() => "yt-dlp.exe",

            // Linux
            Architecture.X64 when OperatingSystem.IsLinux() => "yt-dlp_linux",
            Architecture.Arm64 when OperatingSystem.IsLinux() => "yt-dlp_linux_aarch64",

            // MacOS
            Architecture.X64 when OperatingSystem.IsMacOS() => "yt-dlp_macos_legacy",
            Architecture.Arm64 when OperatingSystem.IsMacOS() => "yt-dlp_macos",
            _ => throw new NotSupportedException($"Architecture of type {RuntimeInformation.OSArchitecture} is not supported by yt-dlp on this OS.")
        };
    }
}