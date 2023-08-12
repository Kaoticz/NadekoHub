namespace NadekoUpdater.Common;

/// <summary>
/// Defines the application's environment data.
/// </summary>
public static class AppStatics
{
    /// <summary>
    /// Defines the default location where the updater configuration and bot instances are stored.
    /// </summary>
#if DEBUG
    public static string DefaultAppConfigDirectoryUri { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "NadekoUpdaterDebug");
#else
    public static string DefaultUserConfigUri { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NadekoUpdater");
#endif

    /// <summary>
    /// Defines the location of the application's configuration file.
    /// </summary>
    public static string AppConfigUri { get; } = Path.Combine(DefaultAppConfigDirectoryUri, "config.json");

    /// <summary>
    /// Defines the location of the application's dependencies.
    /// </summary>
    public static string AppDepsUri { get; } = Path.Combine(DefaultAppConfigDirectoryUri, "deps");

    /// <summary>
    /// Defines the location of the default image for the bot avatar.
    /// </summary>
    public const string BotAvatarPlaceholderUri = "avares://NadekoUpdater/Assets/bot.png";

    /// <summary>
    /// The name for an <see cref="HttpClient"/> that does not automatically follow redirect responses.
    /// </summary>
    public const string NoRedirectClient = "NoRedirect";

    /// <summary>
    /// Generates the location path for a bot instance, given its name.
    /// </summary>
    /// <param name="botName">The name of the bot instance.</param>
    /// <returns>The location path to the bot instance.</returns>
    public static string GenerateBotLocationUri(string botName)
        => Path.Combine(DefaultAppConfigDirectoryUri, botName);
}