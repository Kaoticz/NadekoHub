using NadekoHub.Features.AppConfig.Models;
using NadekoHub.Features.AppConfig.Services.Abstractions;
using NadekoHub.Features.AppWindow.Models;
using System.Text.Json;

namespace NadekoHub.Features.AppConfig.Services;

/// <summary>
/// Defines a service that manages the application's settings.
/// </summary>
public sealed class AppConfigManager : IAppConfigManager
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };
    private readonly AppSettings _appConfig;

    /// <inheritdoc/>
    public ReadOnlyAppSettings AppConfig { get; }

    /// <summary>
    /// Creates a service that manages the application's settings.
    /// </summary>
    public AppConfigManager(AppSettings appConfig, ReadOnlyAppSettings readOnlyAppConfig)
    {
        _appConfig = appConfig;
        AppConfig = readOnlyAppConfig;

        Directory.CreateDirectory(AppStatics.AppDefaultConfigDirectoryUri);  // Create the directory where the app settings will be stored.
    }

    /// <inheritdoc/>
    public async ValueTask<BotEntry> CreateBotEntryAsync(CancellationToken cToken = default)
    {
        var newId = CreateNewId();
        var newPosition = _appConfig.BotEntries.Count is 0 ? 0 : _appConfig.BotEntries.Values.Max(x => x.Position) + 1;
        var newBotName = "NewBot_" + newPosition;
        var newEntry = new BotInstanceInfo(newBotName, Path.Combine(_appConfig.BotsDirectoryUri, newBotName), newPosition);

        if (!_appConfig.BotEntries.TryAdd(newId, newEntry))
            throw new InvalidOperationException($"Could not create a new bot entry with Id {newId}.");

        await SaveAsync(cToken);

        return new(newId, newEntry);
    }

    /// <inheritdoc/>
    public async ValueTask<BotEntry?> DeleteBotEntryAsync(Guid id, CancellationToken cToken = default)
    {
        if (!_appConfig.BotEntries.TryRemove(id, out var removedEntry))
            return null;

        Utilities.TryDeleteDirectory(removedEntry.InstanceDirectoryUri);

        await SaveAsync(cToken);

        return new(id, removedEntry);
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async ValueTask<bool> UpdateBotEntryAsync(Guid id, Func<BotInstanceInfo, BotInstanceInfo> selector, CancellationToken cToken = default)
    {
        if (!_appConfig.BotEntries.TryRemove(id, out var entry))
            return false;

        var updatedEntry = selector(entry);

        _appConfig.BotEntries.TryAdd(id, updatedEntry);

        await SaveAsync(cToken);

        return true;
    }

    /// <inheritdoc/>
    public ValueTask UpdateConfigAsync(Action<AppSettings> action, CancellationToken cToken = default)
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