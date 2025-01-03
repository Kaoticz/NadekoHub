using NadekoHub.Enums;
using NadekoHub.Features.AppWindow.Models;
using System.Collections.Concurrent;

namespace NadekoHub.Features.AppConfig.Models;

/// <summary>
/// Represents the settings of the application.
/// </summary>
/// <remarks>Prefer using <see cref="ReadOnlyAppSettings"/> in dependency injection, if possible.</remarks>
public sealed class AppSettings
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
    /// The theme to be used.
    /// </summary>
    public ThemeType Theme { get; set; } = ThemeType.Auto;

    /// <summary>
    /// Determines whether the application should update itself.
    /// </summary>
    public bool AutomaticUpdates { get; set; } = true;

    /// <summary>
    /// Determines whether the application should be minimized to the system tray when closed.
    /// </summary>
    public bool MinimizeToTray { get; set; } = true;

    /// <summary>
    /// Determines the maximum size a log file can have, in Mb.
    /// </summary>
    public double LogMaxSizeMb { get; set; } = 0.5;

    /// <summary>
    /// Determines the size the application window should be set on startup.
    /// </summary>
    public WindowSize WindowSize { get; set; } = new(WindowConstants.DefaultWindowWidth, WindowConstants.DefaultWindowHeight);

    /// <summary>
    /// A collection of metadata about the bot instances.
    /// </summary>
    public ConcurrentDictionary<Guid, BotInstanceInfo> BotEntries { get; init; } = new();
}