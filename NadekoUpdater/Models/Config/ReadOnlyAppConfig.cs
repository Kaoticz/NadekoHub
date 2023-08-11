namespace NadekoUpdater.Models.Config;

/// <summary>
/// Represents a read-only version of <see cref="AppConfig"/>.
/// </summary>
public sealed class ReadOnlyAppConfig
{
    private readonly AppConfig _appConfig;

    /// <summary>
    /// The absolute path to the directory where the bot instances will be stored.
    /// </summary>
    public string BotsDirectoryUri
        => _appConfig.BotsDirectoryUri;

    /// <summary>
    /// Determines whether the application should be minimized to the system tray when closed.
    /// </summary>
    public bool MinimizeToTray
        => _appConfig.MinimizeToTray;

    /// <summary>
    /// A collection of metadata about the bot instances.
    /// </summary>
    public IReadOnlyDictionary<uint, BotInstanceInfo> BotEntries
        => _appConfig.BotEntries;

    /// <summary>
    /// Initializes a read-only version of <see cref="AppConfig"/>.
    /// </summary>
    /// <param name="appConfig">The application settings to read from.</param>
    public ReadOnlyAppConfig(AppConfig appConfig)
        => _appConfig = appConfig;
}