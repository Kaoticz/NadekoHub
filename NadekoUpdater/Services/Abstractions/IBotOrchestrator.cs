using Kotz.Events;
using NadekoUpdater.Models.EventArguments;

namespace NadekoUpdater.Services.Abstractions;

/// <summary>
/// Represents an object that coordinates multiple running processes of NadekoBot.
/// </summary>
public interface IBotOrchestrator
{
    /// <summary>
    /// Raised when a bot process exits.
    /// </summary>
    event EventHandler<NadekoOrchestrator, BotExitEventArgs>? OnBotExit;

    /// <summary>
    /// Raised when a bot process prints data to stderr.
    /// </summary>
    event EventHandler<NadekoOrchestrator, ProcessStdWriteEventArgs>? OnStderr;

    /// <summary>
    /// Raised when a bot process prints data to stdout.
    /// </summary>
    event EventHandler<NadekoOrchestrator, ProcessStdWriteEventArgs>? OnStdout;

    /// <summary>
    /// Determines whether the bot with the specified
    /// position in the lateral bar is currently running.
    /// </summary>
    /// <param name="botPosition">The bot's position in the lateral bar.</param>
    /// <returns><see langword="true"/> if the bot is running, <see langword="false"/> otherwise.</returns>
    bool IsBotRunning(uint botPosition);

    /// <summary>
    /// Starts the bot with the specified position.
    /// </summary>
    /// <param name="botPosition">The bot's position in the lateral bar.</param>
    /// <returns><see langword="true"/> if the bot successfully started, <see langword="false"/> otherwise.</returns>
    bool Start(uint botPosition);

    /// <summary>
    /// Stops the bot with the specified position.
    /// </summary>
    /// <param name="botPosition">The bot's position in the lateral bar.</param>
    /// <returns><see langword="true"/> if the bot successfully stopped, <see langword="false"/> otherwise.</returns>
    bool Stop(uint botPosition);

    /// <summary>
    /// Stops all bot instances.
    /// </summary>
    /// <returns><see langword="true"/> if at least one bot instance was stopped, <see langword="false"/> otherwise.</returns>
    bool StopAll();
}