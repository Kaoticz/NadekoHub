using NadekoUpdater.Services.Abstractions;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace NadekoUpdater.Services;

/// <summary>
/// Service that checks, downloads, installs, and updates ffmpeg on Linux.
/// </summary>
/// <remarks>Source: https://johnvansickle.com/ffmpeg</remarks>
public sealed partial class FfmpegLinuxResolver : FfmpegResolver
{
    private readonly Regex _ffmpegLatestVersionRegex = FfmpegLatestVersionRegexGenerator();
    private readonly string _tempDirectory = Path.GetTempPath();
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
        return (RuntimeInformation.OSArchitecture is Architecture.X86 or Architecture.Arm64)
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
            ? match.Value
            : throw new InvalidOperationException("Regex did not match the web page content.");
    }

    /// <inheritdoc/>
    public override async ValueTask<(string? OldVersion, string? NewVersion)> InstallOrUpdateAsync(string dependenciesUri, CancellationToken cToken = default)
    {
        var currentVersion = await GetCurrentVersionAsync(cToken);
        var newVersion = await GetLatestVersionAsync(cToken);

        // Update
        if (currentVersion is not null)
        {
            // If the versions are the same, exit.
            if (currentVersion == newVersion)
                return (currentVersion, null);

            Utilities.TryDeleteFile(Path.Combine(dependenciesUri, FileName));
            Utilities.TryDeleteFile(Path.Combine(dependenciesUri, "ffprobe"));
        }

        // Install
        Directory.CreateDirectory(dependenciesUri);

        var tarFileName = $"ffmpeg-release-{((RuntimeInformation.OSArchitecture is Architecture.X64) ? "amd" : "arm")}64-static.tar.xz";
        var http = _httpClientFactory.CreateClient();
        using var downloadStream = await http.GetStreamAsync($"https://johnvansickle.com/ffmpeg/releases/{tarFileName}", cToken);

        // Save tar file to the temporary directory.
        var tarFilePath = Path.Combine(_tempDirectory, tarFileName);
        var tarExtractDir = Path.Combine(_tempDirectory, tarFileName[..^7]);
        using (var fileStream = new FileStream(tarFilePath, FileMode.Create))
            await downloadStream.CopyToAsync(fileStream, cToken);

        // Extract the tar file.
        using var extractProcess = Utilities.StartProcess("tar", $"xf \"{tarFilePath}\" --directory=\"{_tempDirectory}\"");
        await extractProcess.WaitForExitAsync(cToken);

        // Move ffmpeg to the dependencies directory.
        File.Move(Path.Combine(tarExtractDir, FileName), Path.Combine(dependenciesUri, FileName));
        File.Move(Path.Combine(tarExtractDir, "ffprobe"), Path.Combine(dependenciesUri, "ffprobe"));

        // Mark the files as executable.
        using var chmod = Utilities.StartProcess("chmod", $"+x {Path.Combine(dependenciesUri, FileName)} {Path.Combine(dependenciesUri, "ffprobe")}");
        await chmod.WaitForExitAsync(cToken);

        // Cleanup
        File.Delete(tarFilePath);
        Directory.Delete(tarExtractDir, true);

        // Update environment variable
        Utilities.AddPathToPATHEnvar(dependenciesUri);

        return (currentVersion, newVersion);
    }

    [GeneratedRegex(@"release:\s?([\d\.]+)", RegexOptions.Compiled)]
    private static partial Regex FfmpegLatestVersionRegexGenerator();
}