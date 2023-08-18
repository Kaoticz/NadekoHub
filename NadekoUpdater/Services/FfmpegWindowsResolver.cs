using NadekoUpdater.Services.Abstractions;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace NadekoUpdater.Services;

/// <summary>
/// Service that checks, downloads, installs, and updates ffmpeg on Windows.
/// </summary>
/// <remarks>Source: https://github.com/GyanD/codexffmpeg/releases/latest</remarks>
public sealed class FfmpegWindowsResolver : FfmpegResolver
{
    private readonly string _tempDirectory = Path.GetTempPath();
    private bool _isUpdating = false;
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
        return (RuntimeInformation.OSArchitecture is Architecture.X86 or Architecture.X64)
            ? base.CanUpdateAsync(cToken)
            : ValueTask.FromResult<bool?>(false);
    }

    /// <inheritdoc />
    public override async ValueTask<string> GetLatestVersionAsync(CancellationToken cToken = default)
    {
        var http = _httpClientFactory.CreateClient(AppStatics.NoRedirectClient);

        var response = await http.GetAsync("https://github.com/GyanD/codexffmpeg/releases/latest", cToken);

        var lastSlashIndex = response.Headers.Location?.OriginalString.LastIndexOf('/')
            ?? throw new InvalidOperationException("Failed to get the latest yt-dlp version.");

        return response.Headers.Location.OriginalString[(lastSlashIndex + 1)..];
    }

    /// <inheritdoc />
    public override async ValueTask<(string? OldVersion, string? NewVersion)> InstallOrUpdateAsync(string dependenciesUri, CancellationToken cToken = default)
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

            Utilities.TryDeleteFile(Path.Combine(dependenciesUri, FileName));
            Utilities.TryDeleteFile(Path.Combine(dependenciesUri, "ffprobe.exe"));
            //Utilities.TryDeleteFile(Path.Combine(dependenciesUri, "ffplay.exe"));
        }

        // Install
        Directory.CreateDirectory(dependenciesUri);

        var zipFileName = $"ffmpeg-{newVersion}-full_build.zip";
        var http = _httpClientFactory.CreateClient();
        using var downloadStream = await http.GetStreamAsync($"https://github.com/GyanD/codexffmpeg/releases/download/{newVersion}/{zipFileName}", cToken);

        // Save zip file to the temporary directory.
        var zipFilePath = Path.Combine(_tempDirectory, zipFileName);
        var zipExtractDir = Path.Combine(_tempDirectory, zipFileName[..^4]);
        using (var fileStream = new FileStream(zipFilePath, FileMode.Create))
            await downloadStream.CopyToAsync(fileStream, cToken);

        // Extract the zip file.
        ZipFile.ExtractToDirectory(zipFilePath, _tempDirectory);
        
        // Move ffmpeg to the dependencies directory.
        File.Move(Path.Combine(zipExtractDir, "bin", FileName), Path.Combine(dependenciesUri, FileName));
        File.Move(Path.Combine(zipExtractDir, "bin", "ffprobe.exe"), Path.Combine(dependenciesUri, "ffprobe.exe"));
        //File.Move(Path.Combine(zipExtractDir, "bin", "ffplay.exe"), Path.Combine(dependenciesUri, "ffplay.exe"));

        // Cleanup
        File.Delete(zipFilePath);
        Directory.Delete(zipExtractDir, true);

        // Update environment variable
        Utilities.AddPathToPATHEnvar(dependenciesUri);

        _isUpdating = false;
        return (currentVersion, newVersion);
    }
}