namespace NadekoUpdater.Models.EventArguments;

/// <summary>
/// Defines the event arguments when a bot process writes to stdout or stderr.
/// </summary>
public sealed class ProcessStdWriteEventArgs : EventArgs
{
    /// <summary>
    /// The Id of the bot.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// The value that was just written to std.
    /// </summary>
    public string Output { get; }

    /// <summary>
    /// Creates the event arguments when a bot process writes to stdout or stderr.
    /// </summary>
    /// <param name="botId">The Id of the bot.</param>
    /// <param name="output">The value that was just written to std.</param>
    public ProcessStdWriteEventArgs(Guid botId, string output)
    {
        Id = botId;
        Output = output;
    }
}