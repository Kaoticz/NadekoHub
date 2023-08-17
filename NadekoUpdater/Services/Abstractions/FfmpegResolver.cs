namespace NadekoUpdater.Services.Abstractions;

/// <summary>
/// Base class for a service that checks, downloads, installs, and updates ffmpeg.
/// </summary>
public abstract class FfmpegResolver : IFfmpegResolver
{
    private readonly string _programVerifier = (Environment.OSVersion.Platform is PlatformID.Win32NT) ? "where" : "which";

    /// <summary>
    /// The name of the Ffmpeg process.
    /// </summary>
    protected const string FfmpegProcessName = "ffmpeg";

    /// <inheritdoc/>
    public string DependencyName { get; } = "Ffmpeg";

    /// <inheritdoc/>
    public abstract string FileName { get; }

    /// <inheritdoc/>
    public virtual async ValueTask<bool?> CanUpdateAsync(CancellationToken cToken = default)
    {
        // Check where ffmpeg is referenced.
        using var whereProcess = Utilities.StartProcess(_programVerifier, FfmpegProcessName);
        var installationPath = await whereProcess.StandardOutput.ReadToEndAsync(cToken);

        // If ffmpeg is present but not managed by us, just report it is installed.
        if (!string.IsNullOrWhiteSpace(installationPath) && !installationPath.Contains(AppStatics.AppDepsUri, StringComparison.Ordinal))
            return false;

        var currentVer = await GetCurrentVersionAsync(cToken);

        // If ffmpeg or ffprobe are absent, a reinstall needs to be performed.
        if (currentVer is null || !await Utilities.ProgramExistsAsync("ffprobe", cToken))
            return null;       

        var latestVer = await GetLatestVersionAsync(cToken);

        return !latestVer.Equals(currentVer, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public virtual async ValueTask<string?> GetCurrentVersionAsync(CancellationToken cToken = default)
    {
        // If ffmpeg is not accessible from the shell...
        if (!await Utilities.ProgramExistsAsync(FfmpegProcessName, cToken))
        {
            // And doesn't exist in the dependencies folder,
            // report that ffmpeg is not installed.
            if (!File.Exists(Path.Combine(AppStatics.AppDepsUri, FileName)))
                return null;

            // Else, add the dependencies directory to the PATH envar,
            // then try again.
            Utilities.AddPathToPATHEnvar(AppStatics.AppDepsUri);
            return await GetCurrentVersionAsync(cToken);
        }

        using var ffmpeg = Utilities.StartProcess(FfmpegProcessName, "-version");
        var match = AppStatics.FfmpegVersionRegex.Match(await ffmpeg.StandardOutput.ReadLineAsync(cToken) ?? string.Empty);

        return match.Groups[1].Value;
    }

    /// <inheritdoc/>
    public abstract ValueTask<string> GetLatestVersionAsync(CancellationToken cToken = default);

    /// <inheritdoc/>
    public abstract ValueTask<(string? OldVersion, string? NewVersion)> InstallOrUpdateAsync(string dependenciesUri, CancellationToken cToken = default);
}
