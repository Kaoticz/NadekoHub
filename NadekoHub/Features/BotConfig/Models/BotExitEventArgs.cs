namespace NadekoHub.Features.BotConfig.Models;

/// <summary>
/// Defines the event arguments when a bot process exits.
/// </summary>
public sealed class BotExitEventArgs : EventArgs
{
    /// <summary>
    /// The bot's Id.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// The exit code.
    /// </summary>
    public int ExitCode { get; }
    
    /// <summary>
    /// The exit message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Creates the event arguments when a bot process exits.
    /// </summary>
    /// <param name="botId">The bot's Id.</param>
    /// <param name="exitCode">The exit code.</param>
    /// <param name="message">The message for the bot process that just exited.</param>
    public BotExitEventArgs(Guid botId, int exitCode, string message)
    {
        Id = botId;
        ExitCode = exitCode;
        Message = message;
    }
}