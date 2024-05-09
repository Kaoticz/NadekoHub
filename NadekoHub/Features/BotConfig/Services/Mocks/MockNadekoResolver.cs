using NadekoHub.Features.BotConfig.Services.Abstractions;

namespace NadekoHub.Features.BotConfig.Services.Mocks;

/// <summary>
/// Defines a service that pretends to check, download, install, and update a bot instance.
/// </summary>
internal sealed class MockNadekoResolver : IBotResolver
{
    /// <inheritdoc/>
    public string BotName { get; } = "MockBot";

    /// <inheritdoc/>
    public Guid Id { get; } = Guid.Empty;

    /// <inheritdoc/>
    public string DependencyName { get; } = "NadekoBot";

    /// <inheritdoc/>
    public string FileName { get; } = "NadekoBot";

    /// <inheritdoc/>
    public bool IsUpdateInProgress { get; } = false;

    /// <inheritdoc/>
    public ValueTask<bool?> CanUpdateAsync(CancellationToken cToken = default)
        => ValueTask.FromResult<bool?>(false);

    /// <inheritdoc/>
    public ValueTask<string?> CreateBackupAsync()
        => ValueTask.FromResult<string?>(null);

    /// <inheritdoc/>
    public ValueTask<string?> GetCurrentVersionAsync(CancellationToken cToken = default)
        => ValueTask.FromResult<string?>("4.4.4");

    /// <inheritdoc/>
    public ValueTask<string> GetLatestVersionAsync(CancellationToken cToken = default)
        => ValueTask.FromResult("4.4.4");

    /// <inheritdoc/>
    public ValueTask<(string? OldVersion, string? NewVersion)> InstallOrUpdateAsync(string dependenciesUri, CancellationToken cToken = default)
        => ValueTask.FromResult<(string?, string?)>(("4.4.4", null));
}