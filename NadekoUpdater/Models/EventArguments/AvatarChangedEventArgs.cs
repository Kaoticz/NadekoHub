using SkiaSharp;

namespace NadekoUpdater.Models.EventArguments;

/// <summary>
/// Defines the event arguments for when the user sets a new avatar for a bot instance.
/// </summary>
public sealed class AvatarChangedEventArgs : EventArgs
{
    /// <summary>
    /// The Id of the bot.
    /// </summary>
    public Guid BotId { get; }

    /// <summary>
    /// The new avatar.
    /// </summary>
    public SKBitmap Avatar { get; }

    /// <summary>
    /// The absolute path to the avatar's file.
    /// </summary>
    public string AvatarUri { get; }

    /// <summary>
    /// Creates the event arguments for when the user sets a new avatar for a bot instance.
    /// </summary>
    /// <param name="botId">The Id of the bot.</param>
    /// <param name="avatar">The new avatar.</param>
    /// <param name="avatarUri">The absolute path to the avatar's file.</param>
    public AvatarChangedEventArgs(Guid botId, SKBitmap avatar, string avatarUri)
    {
        BotId = botId;
        Avatar = avatar;
        AvatarUri = avatarUri;
    }
}