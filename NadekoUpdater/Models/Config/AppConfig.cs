using System.Collections.Concurrent;

namespace NadekoUpdater.Models.Config;

/// <summary>
/// Represents the settings of the application.
/// </summary>
/// <remarks>Prefer using <see cref="ReadOnlyAppConfig"/> in dependency injection, if possible.</remarks>
public sealed class AppConfig
{
    /// <summary>
    /// The absolute path to the directory where the bot instances will be stored.
    /// </summary>
    public string BotsDirectoryUri { get; set; } = AppStatics.AppDefaultConfigDirectoryUri;

    /// <summary>
    /// The absolute path to the directory where the bot instances are backed up.
    /// </summary>
    public string BotsBackupDirectoryUri { get; set; } = AppStatics.AppDefaultBotBackupDirectoryUri;

    /// <summary>
    /// Determines whether the application should be minimized to the system tray when closed.
    /// </summary>
    public bool MinimizeToTray { get; set; } = true;

    /// <summary>
    /// A collection of metadata about the bot instances.
    /// </summary>
    public ConcurrentDictionary<uint, BotInstanceInfo> BotEntries { get; init; } = new();
}