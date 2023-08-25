namespace NadekoUpdater.Common;

/// <summary>
/// Defines the constants used throughout the whole application.
/// </summary>
public static class AppConstants
{
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
}
