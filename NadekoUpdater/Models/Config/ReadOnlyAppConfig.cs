namespace NadekoUpdater.Models.Config;

/// <summary>
/// Represents a read-only version of <see cref="AppConfig"/>.
/// </summary>
public sealed class ReadOnlyAppConfig
{
    private readonly AppConfig _appConfig;

    /// <summary>
    /// The absolute path to the directory where the bot instances are stored.
    /// </summary>
    public string BotsDirectoryUri
        => _appConfig.BotsDirectoryUri;

    /// <summary>
    /// The absolute path to the directory where the bot instances are backed up.
    /// </summary>
    public string BotsBackupDirectoryUri
        => _appConfig.BotsBackupDirectoryUri;

    /// <summary>
    /// The absolute path to the directory where the bot logs are stored.
    /// </summary>
    public string LogsDirectoryUri
        => _appConfig.LogsDirectoryUri;

    /// <summary>
    /// Determines whether the application should be minimized to the system tray when closed.
    /// </summary>
    public bool MinimizeToTray
        => _appConfig.MinimizeToTray;

    /// <summary>
    /// Determines the maximum size a log file can have, in Mb.
    /// </summary>
    public double LogMaxSizeMb
        => _appConfig.LogMaxSizeMb;

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