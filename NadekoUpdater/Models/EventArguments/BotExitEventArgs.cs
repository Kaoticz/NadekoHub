namespace NadekoUpdater.Models.EventArguments;

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
    /// Creates the event arguments when a bot process exits.
    /// </summary>
    /// <param name="botId">The bot's Id.</param>
    /// <param name="exitCode">The exit code.</param>
    public BotExitEventArgs(Guid botId, int exitCode)
    {
        Id = botId;
        ExitCode = exitCode;
    }
}