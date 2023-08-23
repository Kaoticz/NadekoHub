using NadekoUpdater.Models.Config;
using System.Diagnostics;

namespace NadekoUpdater.Services;

/// <summary>
/// Represents an object that coordinates multiple running processes of NadekoBot.
/// </summary>
public sealed class NadekoOrchestrator
{
    private readonly Dictionary<uint, Process> _runningBots = new();
    private readonly ReadOnlyAppConfig _appConfig;
    private readonly string _fileName = (OperatingSystem.IsWindows()) ? "NadekoBot.exe" : "NadekoBot";

    /// <summary>
    /// Creates an object that coordinates multiple running processes of NadekoBot.
    /// </summary>
    /// <param name="appConfig">The application settings.</param>
    public NadekoOrchestrator(ReadOnlyAppConfig appConfig)
        => _appConfig = appConfig;

    /// <summary>
    /// Determines whether the bot with the specified
    /// position in the lateral bar is currently running.
    /// </summary>
    /// <param name="botPosition">The bot's position in the lateral bar.</param>
    /// <returns><see langword="true"/> if the bot is running, <see langword="false"/> otherwise.</returns>
    public bool IsBotRunning(uint botPosition)
        => _runningBots.ContainsKey(botPosition);

    /// <summary>
    /// Starts the bot with the specified position.
    /// </summary>
    /// <param name="botPosition">The bot's position in the lateral bar.</param>
    /// <returns><see langword="true"/> if the bot successfully started, <see langword="false"/> otherwise.</returns>
    public bool Start(uint botPosition)
    {
        if (_runningBots.ContainsKey(botPosition)
            || !_appConfig.BotEntries.TryGetValue(botPosition, out var botEntry)
            || !File.Exists(Path.Combine(botEntry.InstanceDirectoryUri, _fileName)))
            return false;

        var botProcess = Process.Start(new ProcessStartInfo()
        {
            FileName = Path.Combine(botEntry.InstanceDirectoryUri, _fileName),
            WorkingDirectory = botEntry.InstanceDirectoryUri,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        });
                
        if (botProcess is null)
            return false;

        botProcess.EnableRaisingEvents = true;
        botProcess.Exited += OnExit;

        return _runningBots.TryAdd(botPosition, botProcess);
    }

    /// <summary>
    /// Stops the bot with the specified position.
    /// </summary>
    /// <param name="botPosition">The bot's position in the lateral bar.</param>
    /// <returns><see langword="true"/> if the bot successfully stopped, <see langword="false"/> otherwise.</returns>
    public bool Stop(uint botPosition)
    {
        if (!_runningBots.TryGetValue(botPosition, out var botProcess))
            return false;

        botProcess.Kill(true);
        return true;
    }

    /// <summary>
    /// Finalizes a process when it stops running.
    /// </summary>
    /// <param name="sender">The process.</param>
    /// <param name="eventArgs">The event arguments.</param>
    /// <exception cref="InvalidOperationException">Occurs when <paramref name="sender"/> is not of type <see cref="Process"/>.</exception>
    private void OnExit(object? sender, EventArgs eventArgs)
    {
        var process = (sender as Process) ?? throw new InvalidOperationException($"Sender of type {sender?.GetType().FullName ?? "null"} is not a {nameof(Process)}!");
        var processEntry = _runningBots.First(x => x.Value.Equals(process));

        _runningBots.Remove(processEntry.Key);
        process.Dispose();
    }
}