using Kotz.Events;
using NadekoUpdater.Models.Config;
using NadekoUpdater.Models.EventArguments;
using NadekoUpdater.Services.Abstractions;
using System.Diagnostics;

namespace NadekoUpdater.Services;

/// <summary>
/// Defines an object that coordinates multiple running processes of NadekoBot.
/// </summary>
public sealed class NadekoOrchestrator : IBotOrchestrator
{
    private readonly Dictionary<uint, Process> _runningBots = new();
    private readonly ReadOnlyAppConfig _appConfig;
    private readonly string _fileName = (OperatingSystem.IsWindows()) ? "NadekoBot.exe" : "NadekoBot";

    /// <inheritdoc/>
    public event EventHandler<IBotOrchestrator, BotExitEventArgs>? OnBotExit;

    /// <inheritdoc/>
    public event EventHandler<IBotOrchestrator, ProcessStdWriteEventArgs>? OnStderr;

    /// <inheritdoc/>    
    public event EventHandler<IBotOrchestrator, ProcessStdWriteEventArgs>? OnStdout;   

    /// <summary>
    /// Creates an object that coordinates multiple running processes of NadekoBot.
    /// </summary>
    /// <param name="appConfig">The application settings.</param>
    public NadekoOrchestrator(ReadOnlyAppConfig appConfig)
        => _appConfig = appConfig;


    /// <inheritdoc/>
    public bool IsBotRunning(uint botPosition)
        => _runningBots.ContainsKey(botPosition);


    /// <inheritdoc/>
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
        botProcess.OutputDataReceived += EmitStdout;
        botProcess.ErrorDataReceived += EmitStderr;
        botProcess.Exited += OnExit;
        botProcess.BeginOutputReadLine();
        botProcess.BeginErrorReadLine();

        return _runningBots.TryAdd(botPosition, botProcess);
    }

    /// <inheritdoc/>
    public bool Stop(uint botPosition)
    {
        if (!_runningBots.TryGetValue(botPosition, out var botProcess))
            return false;

        botProcess.Kill(true);
        return true;
    }

    /// <inheritdoc/>
    public bool StopAll()
    {
        var amount = _runningBots.Count;

        foreach (var process in _runningBots.Values)
            try { process.Kill(true); } catch { }

        return amount is not 0;
    }

    /// <summary>
    /// Finalizes a process when it stops running.
    /// </summary>
    /// <param name="sender">The <see cref="Process"/>.</param>
    /// <param name="eventArgs">The event arguments.</param>
    /// <exception cref="InvalidOperationException">Occurs when <paramref name="sender"/> is not of type <see cref="Process"/>.</exception>
    private void OnExit(object? sender, EventArgs eventArgs)
    {
        var (position, process) = _runningBots.First(x => x.Value.Equals(sender));
        OnBotExit?.Invoke(this, new(position, process.ExitCode));

        _runningBots.Remove(position);
        process.CancelOutputRead();
        process.CancelErrorRead();
        process.Dispose();
    }

    /// <summary>
    /// Raises <see cref="OnStdout"/> with its appropriate event arguments.
    /// </summary>
    /// <param name="sender">The <see cref="Process"/>.</param>
    /// <param name="eventArgs">The event arguments.</param>
    private void EmitStdout(object sender, DataReceivedEventArgs eventArgs)
    {
        if (string.IsNullOrWhiteSpace(eventArgs.Data))
            return;

        var (position, _) = _runningBots.First(x => x.Value.Equals(sender));
        var newEventArgs = new ProcessStdWriteEventArgs(position, eventArgs.Data);

        OnStdout?.Invoke(this, newEventArgs);
    }

    /// <summary>
    /// Raises <see cref="OnStderr"/> with its appropriate event arguments.
    /// </summary>
    /// <param name="sender">The <see cref="Process"/>.</param>
    /// <param name="eventArgs">The event arguments.</param>
    private void EmitStderr(object sender, DataReceivedEventArgs eventArgs)
    {
        if (string.IsNullOrWhiteSpace(eventArgs.Data))
            return;

        var (position, _) = _runningBots.First(x => x.Value.Equals(sender));
        var newEventArgs = new ProcessStdWriteEventArgs(position, eventArgs.Data);

        OnStderr?.Invoke(this, newEventArgs);
    }
}