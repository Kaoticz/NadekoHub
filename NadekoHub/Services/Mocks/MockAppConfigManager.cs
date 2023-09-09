using NadekoHub.Models;
using NadekoHub.Models.Config;
using NadekoHub.Services.Abstractions;

namespace NadekoHub.Services.Mocks;

/// <summary>
/// Represents a service that pretends to manage the application's settings.
/// </summary>
internal sealed class MockAppConfigManager : IAppConfigManager
{
    /// <inheritdoc/>
    public ReadOnlyAppConfig AppConfig { get; } = new(new() { BotEntries = new() { [Guid.Empty] = new("MockBot", Path.Combine(AppStatics.AppDefaultBotDirectoryUri, "MockBot"), 0) } });

    /// <inheritdoc/>
    public ValueTask<BotEntry> CreateBotEntryAsync(CancellationToken cToken = default)
        => ValueTask.FromResult(new BotEntry(Guid.Empty, AppConfig.BotEntries[Guid.Empty]));

    /// <inheritdoc/>
    public ValueTask<BotEntry?> DeleteBotEntryAsync(Guid id, CancellationToken cToken = default)
        => ValueTask.FromResult<BotEntry?>(null);

    /// <inheritdoc/>
    public ValueTask<bool> SwapBotEntryAsync(Guid firstBotId, Guid secondBotId, CancellationToken cToken = default)
        => ValueTask.FromResult(false);

    /// <inheritdoc/>
    public ValueTask<bool> UpdateBotEntryAsync(Guid id, Func<BotInstanceInfo, BotInstanceInfo> selector, CancellationToken cToken = default)
        => ValueTask.FromResult(false);

    /// <inheritdoc/>
    public ValueTask UpdateConfigAsync(Action<AppConfig> action, CancellationToken cToken = default)
        => ValueTask.CompletedTask;
}