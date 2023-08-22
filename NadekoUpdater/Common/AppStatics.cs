using Avalonia.Controls;
using Avalonia.Platform;
using System.Text.RegularExpressions;

namespace NadekoUpdater.Common;

/// <summary>
/// Defines the application's environment data.
/// </summary>
public static partial class AppStatics
{
    /// <summary>
    /// Defines the default location where the updater configuration and bot instances are stored.
    /// </summary>
#if DEBUG
    public static string AppDefaultConfigDirectoryUri { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "NadekoUpdaterDebug");
#else
    public static string AppDefaultConfigDirectoryUri { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NadekoUpdater");
#endif

    /// <summary>
    /// Defines the default location where the bot instances are stored.
    /// </summary>
    public static string AppDefaultBotDirectoryUri { get; } = Path.Combine(AppDefaultConfigDirectoryUri, "Bots");
    
    /// <summary>
    /// Defines the default location where the backups of bot instances are stored.
    /// </summary>
    public static string AppDefaultBotBackupDirectoryUri { get; } = Path.Combine(AppDefaultConfigDirectoryUri, "Backups");

    /// <summary>
    /// Defines the location of the application's configuration file.
    /// </summary>
    public static string AppConfigUri { get; } = Path.Combine(AppDefaultConfigDirectoryUri, "config.json");

    /// <summary>
    /// Defines the location of the application's dependencies.
    /// </summary>
    public static string AppDepsUri { get; } = Path.Combine(AppDefaultConfigDirectoryUri, "Dependencies");

    /// <summary>
    /// Matches the version of Ffmpeg from its CLI output.
    /// </summary>
    /// <remarks>Pattern: ^(?:\S+\s+\D*?){2}(git\S+|[\d\.]+)</remarks>
    public static Regex FfmpegVersionRegex { get; } = FfmpegVersionRegexGenerator();

    /// <summary>
    /// Represents the window icon to be used on dialog windows.
    /// </summary>
    public static WindowIcon DialogWindowIcon { get; } = new(AssetLoader.Open(new Uri(ApplicationWindowIcon)));

    /// <summary>
    /// Defines the location of the application window icon.
    /// </summary>
    public const string ApplicationWindowIcon = "avares://NadekoUpdater/Assets/nadekoupdatericon.ico";

    /// <summary>
    /// Defines the location of the default image for the bot avatar.
    /// </summary>
    public const string BotAvatarPlaceholderUri = "avares://NadekoUpdater/Assets/bot.png";

    /// <summary>
    /// The name for an <see cref="HttpClient"/> that does not automatically follow redirect responses.
    /// </summary>
    public const string NoRedirectClient = "NoRedirect";

    [GeneratedRegex(@"^(?:\S+\s+\D*?){2}(git\S+|[\d\.]+)", RegexOptions.Compiled)]
    private static partial Regex FfmpegVersionRegexGenerator();
}