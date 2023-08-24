using NadekoUpdater.Enums;
using System.Text;

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
    /// The accumulated value written to std since the process started.
    /// </summary>
    public StringBuilder AccumulatedOutput { get; }

    /// <summary>
    /// Creates the event arguments when a bot process writes to stdout or stderr.
    /// </summary>
    /// <param name="position">The position of the bot in the lateral bar.</param>
    /// <param name="output">The value that was just written to std.</param>
    /// <param name="accumulatedOutput">The accumulated value written to std since the process started.</param>
    public ProcessStdWriteEventArgs(uint position, string output, StringBuilder accumulatedOutput)
    {
        Position = position;
        Output = output;
        AccumulatedOutput = accumulatedOutput;
    }
}