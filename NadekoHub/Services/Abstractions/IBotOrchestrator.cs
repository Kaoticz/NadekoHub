using NadekoHub.Models.EventArguments;

namespace NadekoHub.Services.Abstractions;

/// <summary>
/// Represents an object that coordinates multiple running processes of NadekoBot.
/// </summary>
public interface IBotOrchestrator
{
    /// <summary>
    /// Raised when a bot process exits.
    /// </summary>
    event EventHandler<IBotOrchestrator, BotExitEventArgs>? OnBotExit;

    /// <summary>
    /// Raised when a bot process prints data to stderr.
    /// </summary>
    event EventHandler<IBotOrchestrator, ProcessStdWriteEventArgs>? OnStderr;

    /// <summary>
    /// Raised when a bot process prints data to stdout.
    /// </summary>
    event EventHandler<IBotOrchestrator, ProcessStdWriteEventArgs>? OnStdout;

    /// <summary>
    /// Determines whether the bot with the specified <paramref name="botId"/>.
    /// </summary>
    /// <param name="botId">The bot's Id.</param>
    /// <returns><see langword="true"/> if the bot is running, <see langword="false"/> otherwise.</returns>
    bool IsBotRunning(Guid botId);

    /// <summary>
    /// Starts the bot with the specified <paramref name="botId"/>.
    /// </summary>
    /// <param name="botId">The bot's Id.</param>
    /// <returns><see langword="true"/> if the bot successfully started, <see langword="false"/> otherwise.</returns>
    bool Start(Guid botId);

    /// <summary>
    /// Stops the bot with the specified <paramref name="botId"/>.
    /// </summary>
    /// <param name="botId">The bot's Id.</param>
    /// <returns><see langword="true"/> if the bot successfully stopped, <see langword="false"/> otherwise.</returns>
    bool Stop(Guid botId);

    /// <summary>
    /// Stops all bot instances.
    /// </summary>
    /// <returns><see langword="true"/> if at least one bot instance was stopped, <see langword="false"/> otherwise.</returns>
    bool StopAll();
}