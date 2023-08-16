namespace NadekoUpdater.Services.Abstractions;

/// <summary>
/// Base class for a service that checks, downloads, installs, and updates ffmpeg.
/// </summary>
public abstract class FfmpegResolver : IFfmpegResolver
{
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
        using var whereProcess = Utilities.StartProcess("where", FfmpegProcessName);
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

    /// <inheritdoc/>
    public virtual async ValueTask<string?> GetCurrentVersionAsync(CancellationToken cToken = default)
    {
        if (!await Utilities.ProgramExistsAsync(FfmpegProcessName, cToken))
            return null;

        using var ffmpeg = Utilities.StartProcess(FfmpegProcessName, "-version");
        var match = AppStatics.FfmpegVersionRegex.Match(await ffmpeg.StandardOutput.ReadLineAsync(cToken) ?? string.Empty);

        return match.Groups[1].Value;
    }

    /// <inheritdoc/>
    public abstract ValueTask<string> GetLatestVersionAsync(CancellationToken cToken = default);

    /// <inheritdoc/>
    public abstract ValueTask<(string? OldVersion, string? NewVersion)> InstallOrUpdateAsync(string dependenciesUri, CancellationToken cToken = default);
}
