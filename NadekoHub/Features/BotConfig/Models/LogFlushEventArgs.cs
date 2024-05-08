namespace NadekoHub.Features.BotConfig.Models;

/// <summary>
/// Defines the event arguments when a log is written to disk.
/// </summary>
public sealed class LogFlushEventArgs : EventArgs
{
    /// <summary>
    /// The absolute path to the recently created log file.
    /// </summary>
    public string FileUri { get; }

    /// <summary>
    /// The size of the log file, in bytes.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// The date the log file was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Creates the event arguments when a log is written to disk.
    /// </summary>
    /// <param name="fileUri">The absolute path to the recently created log file.</param>
    /// <param name="size">The size of the log file, in bytes.</param>
    /// <param name="createdAt">The date the log file was created.</param>
    public LogFlushEventArgs(string fileUri, int size, DateTimeOffset createdAt)
    {
        FileUri = fileUri;
        Size = size;
        CreatedAt = createdAt;
    }
}