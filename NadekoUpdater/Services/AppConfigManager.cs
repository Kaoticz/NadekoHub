using NadekoUpdater.Models;
using NadekoUpdater.Models.Config;
using System.Text.Json;

namespace NadekoUpdater.Services;

/// <summary>
/// Service that manages the application's settings.
/// </summary>
public sealed class AppConfigManager
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };
    private readonly AppConfig _appConfig;

    /// <summary>
    /// The application settings.
    /// </summary>
    public ReadOnlyAppConfig AppConfig { get; }

    /// <summary>
    /// Creates a service that manages the application's settings.
    /// </summary>
    public AppConfigManager(AppConfig appConfig, ReadOnlyAppConfig readOnlyAppConfig)
    {
        _appConfig = appConfig;
        AppConfig = readOnlyAppConfig;

        Directory.CreateDirectory(AppStatics.AppDefaultConfigDirectoryUri);  // Create the directory where the app settings will be stored.
    }

    /// <summary>
    /// Creates a bot entry.
    /// </summary>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns>The bot entry that got created.</returns>
    /// <exception cref="InvalidOperationException">Occurs when the bot entry is not successfully created.</exception>
    public async ValueTask<BotEntry> CreateBotEntryAsync(CancellationToken cToken = default)
    {
        var newId = CreateNewId();
        var newPosition = (_appConfig.BotEntries.Count is 0) ? 0 : _appConfig.BotEntries.Values.Max(x => x.Position) + 1;
        var newBotName = "NewBot_" + newPosition;
        var newEntry = new BotInstanceInfo(newBotName, Path.Combine(_appConfig.BotsDirectoryUri, newBotName), newPosition);

        if (!_appConfig.BotEntries.TryAdd(newId, newEntry))
            throw new InvalidOperationException($"Could not create a new bot entry with Id {newId}.");

        await SaveAsync(cToken);

        return new(newId, newEntry);
    }

    /// <summary>
    /// Deletes a bot entry at the specified <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The Id of the bot.</param>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns>The bot entry that got deleted, <see langword="null"/> otherwise.</returns>
    public async ValueTask<BotEntry?> DeleteBotEntryAsync(Guid id, CancellationToken cToken = default)
    {
        if (!_appConfig.BotEntries.TryRemove(id, out var removedEntry))
            return null;

        Utilities.TryDeleteDirectory(removedEntry.InstanceDirectoryUri);

        await SaveAsync(cToken);

        return new(id, removedEntry);
    }

    /// <summary>
    /// Moves a bot entry in the list.
    /// </summary>
    /// <param name="firstBotId">The bot being swapped.</param>
    /// <param name="secondBotId">The bot to swap with.</param>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if the entry got moved, <see langword="false"/> otherwise.</returns>
    public async ValueTask<bool> SwapBotEntryAsync(Guid firstBotId, Guid secondBotId, CancellationToken cToken = default)
    {
        if (firstBotId == secondBotId
            || !_appConfig.BotEntries.TryGetValue(firstBotId, out var firstBotEntry)
            || !_appConfig.BotEntries.TryGetValue(secondBotId, out var secondBotEntry))
            return false;

        var tempFirstPosition = firstBotEntry.Position;

        _appConfig.BotEntries[firstBotId] = _appConfig.BotEntries[firstBotId] with { Position = secondBotEntry.Position };
        _appConfig.BotEntries[secondBotId] = _appConfig.BotEntries[secondBotId] with { Position = tempFirstPosition };

        await SaveAsync(cToken);

        return true;
    }

    /// <summary>
    /// Changes the bot entry with the specified <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The Id of the bot.</param>
    /// <param name="selector">The changes that should be performed on the entry.</param>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if changes were made on the entry, <see langword="false"/> otherwise.</returns>
    public async ValueTask<bool> UpdateBotEntryAsync(Guid id, Func<BotInstanceInfo, BotInstanceInfo> selector, CancellationToken cToken = default)
    {
        if (!_appConfig.BotEntries.TryRemove(id, out var entry))
            return false;

        var updatedEntry = selector(entry);

        _appConfig.BotEntries.TryAdd(id, updatedEntry);

        await SaveAsync(cToken);

        return true;
    }

    /// <summary>
    /// Changes the application's settings file according to the <paramref name="action"/>.
    /// </summary>
    /// <param name="action">The action to be performed on the configuration file.</param>
    /// <param name="cToken">The cancellation token.</param>
    public ValueTask UpdateConfigAsync(Action<AppConfig> action, CancellationToken cToken = default)
    {
        action(_appConfig);
        return SaveAsync(cToken);
    }

    /// <summary>
    /// Creates a new Id that is known to be unique in the configuration file.
    /// </summary>
    /// <returns>An unique Id.</returns>
    private Guid CreateNewId()
    {
        var id = Guid.NewGuid();

        while (_appConfig.BotEntries.ContainsKey(id))
            id = Guid.NewGuid();

        return id;
    }

    /// <summary>
    /// Saves the bot entries to a configuration file.
    /// </summary>
    /// <param name="cToken">The cancellation token.</param>
    private async ValueTask SaveAsync(CancellationToken cToken = default)
    {
        // Create the directory where the config file will be stored, if it doesn't exist.
        Directory.CreateDirectory(AppStatics.AppDefaultConfigDirectoryUri);

        // Create the configuration file.
        var json = JsonSerializer.Serialize(_appConfig, _jsonSerializerOptions);
        await File.WriteAllTextAsync(AppStatics.AppConfigUri, json, cToken);
    }
}