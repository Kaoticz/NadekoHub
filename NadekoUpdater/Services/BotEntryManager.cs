using NadekoUpdater.Models;
using NadekoUpdater.ViewModels.Controls;
using System.Collections.Concurrent;
using System.Text.Json;

namespace NadekoUpdater.Services;

/// <summary>
/// Service that manages the bot entries in the <see cref="LateralBarViewModel"/>.
/// </summary>
public sealed class BotEntryManager
{
    /// <summary>
    /// Contains the bot entries and their position in the bot list.
    /// </summary>
    public IReadOnlyDictionary<uint, BotInstanceInfo> BotEntries => _botEntries;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };
    private readonly ConcurrentDictionary<uint, BotInstanceInfo> _botEntries;

    /// <summary>
    /// Creates a service that managers bot entries in the <see cref="LateralBarViewModel"/>.
    /// </summary>
    public BotEntryManager()
    {
        Directory.CreateDirectory(AppStatics.DefaultUserConfigUri);
        _botEntries = (File.Exists(AppStatics.BotEntryListConfigUri))
            ? JsonSerializer.Deserialize<ConcurrentDictionary<uint, BotInstanceInfo>>(File.ReadAllText(AppStatics.BotEntryListConfigUri)) ?? new()
            : new();
    }

    /// <summary>
    /// Creates a bot entry.
    /// </summary>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns>The bot entry that got created.</returns>
    /// <exception cref="InvalidOperationException">Occurs when the bot entry is not successfully created.</exception>
    public async ValueTask<BotEntry> CreateEntryAsync(CancellationToken cToken = default)
    {
        var newPosition = (_botEntries.Count is 0) ? 0 : _botEntries.Keys.Max() + 1;
        var newBotName = "NewBot_" + newPosition;
        var newEntry = new BotInstanceInfo(newBotName, AppStatics.GenerateBotLocationUri(newBotName));

        if (!_botEntries.TryAdd(newPosition, newEntry))
            throw new InvalidOperationException($"Could not create a new bot entry at position {newPosition}.");

        await SaveAsync(cToken);

        return new(newPosition, newEntry);
    }

    /// <summary>
    /// Deletes a bot entry at the specified <paramref name="position"/>.
    /// </summary>
    /// <param name="position">The position of the entry.</param>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns>The bot entry that got deleted, <see langword="null"/> otherwise.</returns>
    public async ValueTask<BotEntry?> DeleteEntryAsync(uint position, CancellationToken cToken = default)
    {
        if (!_botEntries.TryRemove(position, out var removedEntry))
            return null;
        else if (Directory.Exists(removedEntry.InstanceDirectoryUri))
            Directory.Delete(removedEntry.InstanceDirectoryUri, true);

        await SaveAsync(cToken);

        return new(position, removedEntry);
    }

    /// <summary>
    /// Moves a bot entry in the list.
    /// </summary>
    /// <param name="oldPosition">The entry's old position.</param>
    /// <param name="newPosition">The entry's new position.</param>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if the entry got moved, <see langword="false"/> otherwise.</returns>
    public async ValueTask<bool> MoveEntryAsync(uint oldPosition, uint newPosition, CancellationToken cToken = default)
    {
        if (oldPosition == newPosition || !_botEntries.TryGetValue(newPosition, out var target) || !_botEntries.TryGetValue(oldPosition, out var source))
            return false;

        _botEntries.TryRemove(oldPosition, out _);
        _botEntries.TryRemove(newPosition, out _);
        _botEntries.TryAdd(oldPosition, target);
        _botEntries.TryAdd(newPosition, source);

        await SaveAsync(cToken);

        return true;
    }

    /// <summary>
    /// Changes the bot entry at the specified <paramref name="position"/>.
    /// </summary>
    /// <param name="position">The position of the bot entry.</param>
    /// <param name="selector">The changes that should be performed on the entry.</param>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if changes were made on the entry, <see langword="false"/> otherwise.</returns>
    public async ValueTask<bool> EditEntryAsync(uint position, Func<BotInstanceInfo, BotInstanceInfo> selector, CancellationToken cToken = default)
    {
        if (!_botEntries.TryRemove(position, out var entry))
            return false;

        var updatedEntry = selector(entry);

        _botEntries.TryAdd(position, updatedEntry);

        await SaveAsync(cToken);

        if (Directory.Exists(entry.InstanceDirectoryUri) && !entry.InstanceDirectoryUri.Equals(updatedEntry.InstanceDirectoryUri, StringComparison.Ordinal))
            Directory.Move(entry.InstanceDirectoryUri, updatedEntry.InstanceDirectoryUri);

        return true;
    }

    /// <summary>
    /// Saves the bot entries to a configuration file.
    /// </summary>
    /// <param name="cToken">The cancellation token.</param>
    private async ValueTask SaveAsync(CancellationToken cToken = default)
    {
        var json = JsonSerializer.Serialize(BotEntries, _jsonSerializerOptions);
        await File.WriteAllTextAsync(AppStatics.BotEntryListConfigUri, json, cToken);
    }
}