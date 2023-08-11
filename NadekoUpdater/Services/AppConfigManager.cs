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

        Directory.CreateDirectory(appConfig.BotsDirectoryUri);  // Create the directory where the bot instances will be stored.
    }

    /// <summary>
    /// Creates a bot entry.
    /// </summary>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns>The bot entry that got created.</returns>
    /// <exception cref="InvalidOperationException">Occurs when the bot entry is not successfully created.</exception>
    public async ValueTask<BotEntry> CreateBotEntryAsync(CancellationToken cToken = default)
    {
        var newPosition = (_appConfig.BotEntries.Count is 0) ? 0 : _appConfig.BotEntries.Keys.Max() + 1;
        var newBotName = "NewBot_" + newPosition;
        var newEntry = new BotInstanceInfo(newBotName, AppStatics.GenerateBotLocationUri(newBotName));

        if (!_appConfig.BotEntries.TryAdd(newPosition, newEntry))
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
    public async ValueTask<BotEntry?> DeleteBotEntryAsync(uint position, CancellationToken cToken = default)
    {
        if (!_appConfig.BotEntries.TryRemove(position, out var removedEntry))
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
    public async ValueTask<bool> MoveBotEntryAsync(uint oldPosition, uint newPosition, CancellationToken cToken = default)
    {
        if (oldPosition == newPosition || !_appConfig.BotEntries.TryGetValue(newPosition, out var target) || !_appConfig.BotEntries.TryGetValue(oldPosition, out var source))
            return false;

        _appConfig.BotEntries.TryRemove(oldPosition, out _);
        _appConfig.BotEntries.TryRemove(newPosition, out _);
        _appConfig.BotEntries.TryAdd(oldPosition, target);
        _appConfig.BotEntries.TryAdd(newPosition, source);

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
    public async ValueTask<bool> EditBotEntryAsync(uint position, Func<BotInstanceInfo, BotInstanceInfo> selector, CancellationToken cToken = default)
    {
        if (!_appConfig.BotEntries.TryRemove(position, out var entry))
            return false;

        var updatedEntry = selector(entry);

        _appConfig.BotEntries.TryAdd(position, updatedEntry);

        await SaveAsync(cToken);

        if (Directory.Exists(entry.InstanceDirectoryUri) && !entry.InstanceDirectoryUri.Equals(updatedEntry.InstanceDirectoryUri, StringComparison.Ordinal))
            Directory.Move(entry.InstanceDirectoryUri, updatedEntry.InstanceDirectoryUri);

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
    /// Saves the bot entries to a configuration file.
    /// </summary>
    /// <param name="cToken">The cancellation token.</param>
    private async ValueTask SaveAsync(CancellationToken cToken = default)
    {
        // Create the directory where the config file will be stored, if it doesn't exist.
        Directory.CreateDirectory(AppStatics.DefaultAppConfigDirectoryUri);

        // Create the configuration file.
        var json = JsonSerializer.Serialize(_appConfig, _jsonSerializerOptions);
        await File.WriteAllTextAsync(AppStatics.AppConfigUri, json, cToken);
    }
}