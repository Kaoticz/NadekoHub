using Kotz.Utilities;
using NadekoHub.Features.AppConfig.Services.Abstractions;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace NadekoHub.Features.AppConfig.Services;

/// <summary>
/// Service that checks, downloads, installs, and updates ffmpeg on Windows.
/// </summary>
/// <remarks>Source: https://github.com/GyanD/codexffmpeg/releases/latest</remarks>
[SupportedOSPlatform("windows")]
public sealed class FfmpegWindowsResolver : FfmpegResolver
{
    private readonly string _tempDirectory = Path.GetTempPath();
    private bool _isUpdating;
    private readonly IHttpClientFactory _httpClientFactory;

    /// <inheritdoc />
    public override string FileName { get; } = "ffmpeg.exe";

    /// <summary>
    /// Creates a service that checks, downloads, installs, and updates ffmpeg on Windows.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    public FfmpegWindowsResolver(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    /// <inheritdoc/>
    public override ValueTask<bool?> CanUpdateAsync(CancellationToken cToken = default)
    {
        // I could not find any ARM build of ffmpeg for Windows.
        return RuntimeInformation.OSArchitecture is Architecture.X64
            ? base.CanUpdateAsync(cToken)
            : ValueTask.FromResult<bool?>(false);
    }

    /// <inheritdoc />
    public override async ValueTask<string> GetLatestVersionAsync(CancellationToken cToken = default)
    {
        var http = _httpClientFactory.CreateClient(AppConstants.GithubClient);

        var response = await http.GetAsync("https://github.com/GyanD/codexffmpeg/releases/latest", cToken);

        var lastSlashIndex = response.Headers.Location?.OriginalString.LastIndexOf('/')
            ?? throw new InvalidOperationException("Failed to get the latest yt-dlp version.");

        return response.Headers.Location.OriginalString[(lastSlashIndex + 1)..];
    }

    /// <inheritdoc />
    public override async ValueTask<(string? OldVersion, string? NewVersion)> InstallOrUpdateAsync(string installationUri, CancellationToken cToken = default)
    {
        if (_isUpdating)
            return (null, null);

        _isUpdating = true;
        var currentVersion = await GetCurrentVersionAsync(cToken);
        var newVersion = await GetLatestVersionAsync(cToken);

        // Update
        if (currentVersion is not null)
        {
            // If the versions are the same, exit.
            if (currentVersion == newVersion)
            {
                _isUpdating = false;
                return (currentVersion, null);
            }

            KotzUtilities.TryDeleteFile(Path.Join(installationUri, FileName));
            KotzUtilities.TryDeleteFile(Path.Join(installationUri, "ffprobe.exe"));
            //KotzUtilities.TryDeleteFile(Path.Join(dependenciesUri, "ffplay.exe"));
        }

        // Install
        Directory.CreateDirectory(installationUri);

        var zipFileName = $"ffmpeg-{newVersion}-full_build.zip";
        var http = _httpClientFactory.CreateClient();
        await using var downloadStream = await http.GetStreamAsync($"https://github.com/GyanD/codexffmpeg/releases/download/{newVersion}/{zipFileName}", cToken);

        // Save zip file to the temporary directory.
        var zipFilePath = Path.Join(_tempDirectory, zipFileName);
        var zipExtractDir = Path.Join(_tempDirectory, zipFileName[..^4]);
        await using (var fileStream = new FileStream(zipFilePath, FileMode.Create))
            await downloadStream.CopyToAsync(fileStream, cToken);

        // Schedule installation to the thread-pool because ffmpeg is pretty
        // large and doing I/O with it can potentially block the UI thread.
        await Task.Run(() =>
        {
            // Extract the zip file.
            ZipFile.ExtractToDirectory(zipFilePath, _tempDirectory);

            // Move ffmpeg to the dependencies directory.
            KotzUtilities.TryMoveFile(Path.Join(zipExtractDir, "bin", FileName), Path.Join(installationUri, FileName), true);
            KotzUtilities.TryMoveFile(Path.Join(zipExtractDir, "bin", "ffprobe.exe"), Path.Join(installationUri, "ffprobe.exe"), true);
            //KotzUtilities.TryMoveFile(Path.Join(zipExtractDir, "bin", "ffplay.exe"), Path.Join(dependenciesUri, "ffplay.exe"));

            // Cleanup
            KotzUtilities.TryDeleteFile(zipFilePath);
            KotzUtilities.TryDeleteDirectory(zipExtractDir);
        }, cToken);

        // Update environment variable
        KotzUtilities.AddPathToPATHEnvar(installationUri);

        _isUpdating = false;
        return (currentVersion, newVersion);
    }
}