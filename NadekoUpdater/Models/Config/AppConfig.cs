using System.Collections.Concurrent;

namespace NadekoUpdater.Models.Config;

/// <summary>
/// Represents the settings of the application.
/// </summary>
/// <remarks>Prefer using <see cref="ReadOnlyAppConfig"/> in dependency injection, if possible.</remarks>
public sealed class AppConfig
{
    /// <summary>
    /// The absolute path to the directory where the bot instances are stored.
    /// </summary>
    public string BotsDirectoryUri { get; set; } = AppStatics.AppDefaultBotDirectoryUri;

    /// <summary>
    /// The absolute path to the directory where the bot instances are backed up.
    /// </summary>
    public string BotsBackupDirectoryUri { get; set; } = AppStatics.AppDefaultBotBackupDirectoryUri;

    /// <summary>
    /// The absolute path to the directory where the bot logs are stored.
    /// </summary>
    public string LogsDirectoryUri { get; set; } = AppStatics.AppDefaultLogDirectoryUri;

    /// <summary>
    /// Determines whether the application should be minimized to the system tray when closed.
    /// </summary>
    public bool MinimizeToTray { get; set; } = true;

    /// <summary>
    /// Determines the maximum size a log file can have, in Mb.
    /// </summary>
    public double LogMaxSizeMb { get; set; } = 0.5;

    /// <summary>
    /// A collection of metadata about the bot instances.
    /// </summary>
    public ConcurrentDictionary<uint, BotInstanceInfo> BotEntries { get; init; } = new();
}