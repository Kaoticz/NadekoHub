using NadekoUpdater.Services.Abstractions;
using System.Diagnostics;
using System.IO.Compression;

namespace NadekoUpdater.Services;

/// <summary>
/// Service that checks, downloads, installs, and updates ffmpeg.
/// </summary>
/// <remarks>Source: https://github.com/GyanD/codexffmpeg/releases/latest</remarks>
public sealed class FfmpegWindowsResolver : IFfmpegResolver
{
    private const string _ffmpegProcessName = "ffmpeg";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _tempDirectory = Path.GetTempPath();

    /// <inheritdoc />
    public string DependencyName { get; } = "Ffmpeg";

    /// <inheritdoc />
    public string FileName { get; } = "ffmpeg.exe";

    /// <summary>
    /// Creates a service that checks, downloads, installs, and updates ffmpeg.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    public FfmpegWindowsResolver(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    /// <inheritdoc />
    public async ValueTask<bool?> CanUpdateAsync(CancellationToken cToken = default)
    {
        // Check where ffmpeg is referenced.
        using var whereProcess = Utilities.StartProcess("where", _ffmpegProcessName);
        var installationPath = await whereProcess.StandardOutput.ReadToEndAsync(cToken);

        // If ffmpeg is present but not managed by us, just report it is installed.
        if (!string.IsNullOrWhiteSpace(installationPath) && !installationPath.Contains(AppStatics.AppDepsUri, StringComparison.Ordinal))
            return false;
        
        var currentVer = await GetCurrentVersionAsync(cToken);

        if (currentVer is null)
            return null;

        var latestVer = await GetLatestVersionAsync(cToken);

        return !latestVer.Equals(currentVer, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public async ValueTask<string?> GetCurrentVersionAsync(CancellationToken cToken = default)
    {
        if (!await Utilities.ProgramExistsAsync(_ffmpegProcessName, cToken))
            return null;

        using var ffmpeg = Utilities.StartProcess(_ffmpegProcessName, "-version");
        var match = AppStatics.FfmpegVersionRegex.Match(await ffmpeg.StandardOutput.ReadLineAsync(cToken) ?? string.Empty);

        return match.Groups[1].Value;
    }

    /// <inheritdoc />
    public async ValueTask<string> GetLatestVersionAsync(CancellationToken cToken = default)
    {
        using var http = _httpClientFactory.CreateClient(AppStatics.NoRedirectClient);

        var response = await http.GetAsync("https://github.com/GyanD/codexffmpeg/releases/latest", cToken);

        var lastSlashIndex = response.Headers.Location?.OriginalString.LastIndexOf('/')
            ?? throw new InvalidOperationException("Failed to get the latest yt-dlp version.");

        return response.Headers.Location.OriginalString[(lastSlashIndex + 1)..];
    }

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

            File.Delete(Path.Combine(AppStatics.AppDepsUri, FileName));
            File.Delete(Path.Combine(AppStatics.AppDepsUri, "ffprobe.exe"));
            //File.Delete(Path.Combine(AppStatics.AppDepsUri, "ffplay.exe"));
        }

        // Install
        Directory.CreateDirectory(dependenciesUri);

        var zipFileName = $"ffmpeg-{newVersion}-full_build.zip";
        using var http = _httpClientFactory.CreateClient();
        using var downloadStream = await http.GetStreamAsync($"https://github.com/GyanD/codexffmpeg/releases/download/{newVersion}/{zipFileName}", cToken);

        // Save zip file to the temporary directory.
        var zipFilePath = Path.Combine(_tempDirectory, zipFileName);
        var zipExtractDir = Path.Combine(_tempDirectory, zipFileName[..^4]);
        using (var fileStream = new FileStream(zipFilePath, FileMode.Create))
            await downloadStream.CopyToAsync(fileStream, cToken);

        // Extract the zip file.
        ZipFile.ExtractToDirectory(zipFilePath, _tempDirectory);
        
        // Move ffmpeg to the dependencies directory.
        File.Move(Path.Combine(zipExtractDir, "bin", FileName), Path.Combine(AppStatics.AppDepsUri, FileName));
        File.Move(Path.Combine(zipExtractDir, "bin", "ffprobe.exe"), Path.Combine(AppStatics.AppDepsUri, "ffprobe.exe"));
        //File.Move(Path.Combine(zipExtractDir, "bin", "ffplay.exe"), Path.Combine(AppStatics.AppDepsUri, "ffplay.exe"));

        // Cleanup
        File.Delete(zipFilePath);
        Directory.Delete(zipExtractDir, true);

        // Update environment variable
        Utilities.AddPathToPATHEnvar(dependenciesUri);

        return (currentVersion, newVersion);
    }
}