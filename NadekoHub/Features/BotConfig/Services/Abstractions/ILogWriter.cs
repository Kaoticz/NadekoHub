using NadekoHub.Features.BotConfig.Models;
using System.Diagnostics.CodeAnalysis;

namespace NadekoHub.Features.BotConfig.Services.Abstractions;

/// <summary>
/// Represents a service that writes logs of bot instances to the disk.
/// </summary>
public interface ILogWriter
{
    /// <summary>
    /// Raised when a log file is created.
    /// </summary>
    event EventHandler<ILogWriter, LogFlushEventArgs>? OnLogCreated;

    /// <summary>
    /// Writes the logs of all bots to a log file.
    /// </summary>
    /// <param name="removeFromMemory">
    /// <see langword="true"/> if the backing storage for the bot's logs should be removed from memory, <see langword="false"/> otherwise.
    /// </param>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if at least one log file was created, <see langword="false"/> otherwise.</returns>
    Task<bool> FlushAllAsync(bool removeFromMemory = false, CancellationToken cToken = default);

    /// <summary>
    /// Writes the logs of the specified bot to a log file.
    /// </summary>
    /// <param name="botId">The Id of the bot.</param>
    /// <param name="removeFromMemory">
    /// <see langword="true"/> if the backing storage for the bot's logs should be removed from memory, <see langword="false"/> otherwise.
    /// </param>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if the log file was successfully created, <see langword="false"/> otherwise.</returns>
    Task<bool> FlushAsync(Guid botId, bool removeFromMemory = false, CancellationToken cToken = default);

    /// <summary>
    /// Safely adds a <paramref name="message"/> to the log of the bot with the specified <paramref name="botId"/>.
    /// </summary>
    /// <param name="botId">The Id of the bot.</param>
    /// <param name="message">The message to be appended to the log.</param>
    /// <returns><see langword="true"/> if the <paramref name="message"/> was successfully added to the log, <see langword="false"/> otherwise.</returns>
    bool TryAdd(Guid botId, string message);

    /// <summary>
    /// Safely gets the logs of the bot with the specified <paramref name="botId"/>.
    /// </summary>
    /// <param name="botId">The Id of the bot.</param>
    /// <param name="log">The log of the bot.</param>
    /// <returns><see langword="true"/> if the <paramref name="log"/> was successfully retrieved, <see langword="false"/> otherwise.</returns>
    bool TryRead(Guid botId, [MaybeNullWhen(false)] out string log);
}