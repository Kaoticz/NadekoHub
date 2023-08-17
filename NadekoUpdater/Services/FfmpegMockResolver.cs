using NadekoUpdater.Services.Abstractions;

namespace NadekoUpdater.Services;

/// <summary>
/// Service that pretends to check, download, install, and update ffmpeg.
/// </summary>
public sealed class FfmpegMockResolver : FfmpegResolver
{
    private const string _currentVersion = "6.0";

    /// <inheritdoc/>
    public override string FileName { get; } = "ffmpeg";

    /// <inheritdoc/>
    public override ValueTask<bool?> CanUpdateAsync(CancellationToken cToken = default)
        => ValueTask.FromResult<bool?>(false);

    /// <inheritdoc/>
    public override ValueTask<string?> GetCurrentVersionAsync(CancellationToken cToken = default)
        => ValueTask.FromResult<string?>(_currentVersion);

    /// <inheritdoc/>
    public override ValueTask<string> GetLatestVersionAsync(CancellationToken cToken = default)
        => ValueTask.FromResult(_currentVersion);

    /// <inheritdoc/>
    public override ValueTask<(string? OldVersion, string? NewVersion)> InstallOrUpdateAsync(string dependenciesUri, CancellationToken cToken = default)
        => ValueTask.FromResult<(string?, string?)>((_currentVersion, null));
}