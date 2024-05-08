using NadekoHub.Features.AppConfig.Models;
using NadekoHub.Features.AppWindow.Models;

namespace NadekoHub.Features.AppConfig.Services.Abstractions;

/// <summary>
/// Represents a service that manages the application's settings.
/// </summary>
public interface IAppConfigManager
{
    /// <summary>
    /// The application settings.
    /// </summary>
    ReadOnlyAppSettings AppConfig { get; }

    /// <summary>
    /// Creates a bot entry.
    /// </summary>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns>The bot entry that got created.</returns>
    /// <exception cref="InvalidOperationException">Occurs when the bot entry is not successfully created.</exception>
    ValueTask<BotEntry> CreateBotEntryAsync(CancellationToken cToken = default);

    /// <summary>
    /// Deletes a bot entry at the specified <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The Id of the bot.</param>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns>The bot entry that got deleted, <see langword="null"/> otherwise.</returns>
    ValueTask<BotEntry?> DeleteBotEntryAsync(Guid id, CancellationToken cToken = default);

    /// <summary>
    /// Moves a bot entry in the list.
    /// </summary>
    /// <param name="firstBotId">The bot being swapped.</param>
    /// <param name="secondBotId">The bot to swap with.</param>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if the entry got moved, <see langword="false"/> otherwise.</returns>
    ValueTask<bool> SwapBotEntryAsync(Guid firstBotId, Guid secondBotId, CancellationToken cToken = default);

    /// <summary>
    /// Changes the bot entry with the specified <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The Id of the bot.</param>
    /// <param name="selector">The changes that should be performed on the entry.</param>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if changes were made on the entry, <see langword="false"/> otherwise.</returns>
    ValueTask<bool> UpdateBotEntryAsync(Guid id, Func<BotInstanceInfo, BotInstanceInfo> selector, CancellationToken cToken = default);

    /// <summary>
    /// Changes the application's settings file according to the <paramref name="action"/>.
    /// </summary>
    /// <param name="action">The action to be performed on the configuration file.</param>
    /// <param name="cToken">The cancellation token.</param>
    ValueTask UpdateConfigAsync(Action<AppSettings> action, CancellationToken cToken = default);
}