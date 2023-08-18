using NadekoUpdater.Models.Api;
using NadekoUpdater.Services.Abstractions;
using System.IO.Compression;

namespace NadekoUpdater.Services;

/// <summary>
/// Service that checks, downloads, installs, and updates ffmpeg on MacOS.
/// </summary>
/// <remarks>Source: https://evermeet.cx/ffmpeg</remarks>
public sealed class FfmpegMacResolver : FfmpegResolver
{
    private const string _apiFfmpegInfoEndpoint = "https://evermeet.cx/ffmpeg/info/ffmpeg/release";
    private const string _apiFfprobeInfoEndpoint = "https://evermeet.cx/ffmpeg/info/ffprobe/release";
    private readonly string _tempDirectory = Path.GetTempPath();
    private bool _isUpdating = false;
    private readonly IHttpClientFactory _httpClientFactory;

    /// <inheritdoc/>
    public override string FileName { get; } = "ffmpeg";

    /// <summary>
    /// Creates a service that checks, downloads, installs, and updates ffmpeg on MacOS.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    public FfmpegMacResolver(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    /// <inheritdoc/>
    public override async ValueTask<string> GetLatestVersionAsync(CancellationToken cToken = default)
    {
        var http = _httpClientFactory.CreateClient();
        var response = await http.CallApiAsync<EvermeetInfo>(_apiFfmpegInfoEndpoint, cToken);

        return response.Version;
    }

    /// <inheritdoc/>
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
            Utilities.TryDeleteFile(Path.Combine(dependenciesUri, "ffprobe"));
        }

        // Install
        Directory.CreateDirectory(dependenciesUri);

        var http = _httpClientFactory.CreateClient();
        var ffmpegResponse = await http.CallApiAsync<EvermeetInfo>(_apiFfmpegInfoEndpoint, cToken);
        var ffprobeResponse = await http.CallApiAsync<EvermeetInfo>(_apiFfprobeInfoEndpoint, cToken);

        await Task.WhenAll(
            InstallDependencyAsync(ffmpegResponse, dependenciesUri, cToken),
            InstallDependencyAsync(ffprobeResponse, dependenciesUri, cToken)
        );

        // Update environment variable
        Utilities.AddPathToPATHEnvar(dependenciesUri);

        _isUpdating = false;
        return (currentVersion, newVersion);
    }

    /// <summary>
    /// Install the dependency provided by <paramref name="downloadInfo"/>.
    /// </summary>
    /// <param name="downloadInfo">The dependency to be installed.</param>
    /// <param name="dependenciesUri">The absolute path to the directory where the dependency should be installed to.</param>
    /// <param name="cToken">The cancellation token.</param>
    private async Task InstallDependencyAsync(EvermeetInfo downloadInfo, string dependenciesUri, CancellationToken cToken = default)
    {
        var http = _httpClientFactory.CreateClient();
        var downloadUrl = downloadInfo.Download["zip"].Url;
        var zipFileName = downloadUrl[(downloadUrl.LastIndexOf('/') + 1)..];
        var zipFilePath = Path.Combine(_tempDirectory, zipFileName);

        // Download the zip file and save it to the temporary directory.
        using var zipStream = await http.GetStreamAsync(downloadUrl, cToken);

        using (var fileStream = new FileStream(zipFilePath, FileMode.Create))
            await zipStream.CopyToAsync(fileStream, cToken);

        // Extract the zip file.
        ZipFile.ExtractToDirectory(zipFileName, _tempDirectory);

        // Move the dependency binary.
        var destinationUri = Path.Combine(dependenciesUri, downloadInfo.Name);
        File.Move(Path.Combine(_tempDirectory, downloadInfo.Name), destinationUri);

        // Mark binary as executable.
        using var chmod = Utilities.StartProcess("chmod", $"+x {destinationUri}");
        await chmod.WaitForExitAsync(cToken);

        // Cleanup.
        File.Delete(zipFilePath);
    }
}
