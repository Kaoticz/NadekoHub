namespace NadekoUpdater.Models.EventArguments;

/// <summary>
/// Defines the event arguments when a bot process writes to stdout or stderr.
/// </summary>
public sealed class ProcessStdWriteEventArgs : EventArgs
{
    /// <summary>
    /// The position of the bot in the lateral bar.
    /// </summary>
    public uint Position { get; }

    /// <summary>
    /// The value that was just written to std.
    /// </summary>
    public string Output { get; }

    /// <summary>
    /// Creates the event arguments when a bot process writes to stdout or stderr.
    /// </summary>
    /// <param name="position">The position of the bot in the lateral bar.</param>
    /// <param name="output">The value that was just written to std.</param>
    public ProcessStdWriteEventArgs(uint position, string output)
    {
        Position = position;
        Output = output;
    }
}