namespace NadekoUpdater.Models.EventArguments;

/// <summary>
/// Defines the event arguments when a bot process exits.
/// </summary>
public sealed class BotExitEventArgs : EventArgs
{
    /// <summary>
    /// The position of the bot in the lateral bar.
    /// </summary>
    public uint Position { get; }

    /// <summary>
    /// The exit code.
    /// </summary>
    public int ExitCode { get; }

    /// <summary>
    /// Creates the event arguments when a bot process exits.
    /// </summary>
    /// <param name="position">The position of the bot in the lateral bar.</param>
    /// <param name="exitCode">The exit code.</param>
    public BotExitEventArgs(uint position, int exitCode)
    {
        Position = position;
        ExitCode = exitCode;
    }
}