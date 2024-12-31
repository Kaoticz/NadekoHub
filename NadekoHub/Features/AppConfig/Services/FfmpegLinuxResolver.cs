using Kotz.Utilities;
using NadekoHub.Features.AppConfig.Services.Abstractions;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;

namespace NadekoHub.Features.AppConfig.Services;

/// <summary>
/// Service that checks, downloads, installs, and updates ffmpeg on Linux.
/// </summary>
/// <remarks>Source: https://johnvansickle.com/ffmpeg</remarks>
[SupportedOSPlatform("linux")]
public sealed partial class FfmpegLinuxResolver : FfmpegResolver
{
    private readonly Regex _ffmpegLatestVersionRegex = FfmpegLatestVersionRegexGenerator();
    private readonly string _tempDirectory = Path.GetTempPath();
    private bool _isUpdating;
    private readonly IHttpClientFactory _httpClientFactory;

    /// <inheritdoc/>
    public override string FileName { get; } = "ffmpeg";

    /// <summary>
    /// Creates a service that checks, downloads, installs, and updates ffmpeg on Linux.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    public FfmpegLinuxResolver(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    /// <inheritdoc/>
    public override ValueTask<bool?> CanUpdateAsync(CancellationToken cToken = default)
    {
        return (RuntimeInformation.OSArchitecture is Architecture.X64 or Architecture.Arm64)
            ? base.CanUpdateAsync(cToken)
            : ValueTask.FromResult<bool?>(false);
    }

    /// <inheritdoc/>
    public override async ValueTask<string> GetLatestVersionAsync(CancellationToken cToken = default)
    {
        var http = _httpClientFactory.CreateClient();
        var pageContent = await http.GetStringAsync("https://johnvansickle.com/ffmpeg", cToken);
        var match = _ffmpegLatestVersionRegex.Match(pageContent);

        return (match.Success)
            ? match.Groups[1].Value
            : throw new InvalidOperationException("Regex did not match the web page content.");
    }

    /// <inheritdoc/>
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
                return (currentVersion, null);

            KotzUtilities.TryDeleteFile(Path.Join(installationUri, FileName));
            KotzUtilities.TryDeleteFile(Path.Join(installationUri, "ffprobe"));
        }

        // Install
        Directory.CreateDirectory(installationUri);

        var architecture = (RuntimeInformation.OSArchitecture is Architecture.X64) ? "amd" : "arm";
        var tarFileName = $"ffmpeg-release-{architecture}64-static.tar.xz";
        var http = _httpClientFactory.CreateClient();
        await using var downloadStream = await http.GetStreamAsync($"https://johnvansickle.com/ffmpeg/releases/{tarFileName}", cToken);

        // Save tar file to the temporary directory.
        var tarFilePath = Path.Join(_tempDirectory, tarFileName);
        var tarExtractDir = Path.Join(_tempDirectory, $"ffmpeg-{newVersion}-{architecture}64-static");
        await using (var fileStream = new FileStream(tarFilePath, FileMode.Create))
            await downloadStream.CopyToAsync(fileStream, cToken);

        // Extract the tar file.
        using var extractProcess = KotzUtilities.StartProcess("tar", ["xf", tarFilePath, $"--directory=\"{_tempDirectory}\""]);
        await extractProcess.WaitForExitAsync(cToken);

        // Move ffmpeg to the dependencies directory.
        KotzUtilities.TryMoveFile(Path.Join(tarExtractDir, FileName), Path.Join(installationUri, FileName), true);
        KotzUtilities.TryMoveFile(Path.Join(tarExtractDir, "ffprobe"), Path.Join(installationUri, "ffprobe"), true);

        // Mark the files as executable.
        using var chmod = KotzUtilities.StartProcess("chmod", ["+x", Path.Join(installationUri, FileName), Path.Join(installationUri, "ffprobe")]);
        await chmod.WaitForExitAsync(cToken);

        // Cleanup
        KotzUtilities.TryDeleteFile(tarFilePath);
        KotzUtilities.TryDeleteDirectory(tarExtractDir);

        // Update environment variable
        KotzUtilities.AddPathToPATHEnvar(installationUri);

        _isUpdating = false;
        return (currentVersion, newVersion);
    }

    [GeneratedRegex(@"release:\s?([\d\.]+)", RegexOptions.Compiled)]
    private static partial Regex FfmpegLatestVersionRegexGenerator();
}