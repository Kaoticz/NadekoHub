namespace NadekoHub.Common;

/// <summary>
/// Defines the constants used throughout the whole application.
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Defines the location of the default image for the bot avatar.
    /// </summary>
    public const string BotAvatarUri = "avares://NadekoHub/Assets/nadeko.png";

    /// <summary>
    /// The name for an <see cref="HttpClient"/> that does not automatically follow redirect responses.
    /// </summary>
    public const string NoRedirectClient = "NoRedirect";

    /// <summary>
    /// The name for an <see cref="HttpClient"/> that makes calls to the Github API.
    /// </summary>
    public const string GithubClient = "GithubClient";
}