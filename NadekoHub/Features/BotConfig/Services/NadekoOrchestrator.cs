using NadekoHub.Features.AppConfig.Models;
using NadekoHub.Features.BotConfig.Models;
using NadekoHub.Features.BotConfig.Services.Abstractions;
using System.Diagnostics;

namespace NadekoHub.Features.BotConfig.Services;

/// <summary>
/// Defines an object that coordinates multiple running processes of NadekoBot.
/// </summary>
public sealed class NadekoOrchestrator : IBotOrchestrator
{
    private readonly ReadOnlyAppSettings _appConfig;
    private readonly ILogWriter _logWriter;
    private readonly Dictionary<Guid, Process> _runningBots = new();
    private readonly string _fileName = OperatingSystem.IsWindows() ? "NadekoBot.exe" : "NadekoBot";

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
    /// <param name="logWriter">The service that writes bot logs to disk.</param>
    public NadekoOrchestrator(ReadOnlyAppSettings appConfig, ILogWriter logWriter)
    {
        _appConfig = appConfig;
        _logWriter = logWriter;
    }

    /// <inheritdoc/>
    public bool IsBotRunning(Guid botId)
        => _runningBots.ContainsKey(botId);

    /// <inheritdoc/>
    public bool StartBot(Guid botId)
    {
        if (_runningBots.ContainsKey(botId)
            || !_appConfig.BotEntries.TryGetValue(botId, out var botEntry)
            || !File.Exists(Path.Join(botEntry.InstanceDirectoryUri, _fileName)))
            return false;

        var botProcess = Process.Start(new ProcessStartInfo()
        {
            FileName = Path.Join(botEntry.InstanceDirectoryUri, _fileName),
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

        return _runningBots.TryAdd(botId, botProcess);
    }

    /// <inheritdoc/>
    public bool StopBot(Guid botId)
    {
        if (!_runningBots.TryGetValue(botId, out var botProcess))
            return false;

        botProcess.Kill(true);
        return true;
    }

    /// <inheritdoc/>
    public bool StopAllBots()
    {
        var amount = _runningBots.Count;

        // ReSharper disable once EmptyGeneralCatchClause
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
        var (id, process) = _runningBots.First(x => x.Value.Equals(sender));
        var message = Environment.NewLine
            + $"{_appConfig.BotEntries[id].Name} stopped. Status code: {process.ExitCode}"
            + Environment.NewLine;

        _logWriter.TryAdd(id, message);
        OnBotExit?.Invoke(this, new(id, process.ExitCode, message));

        _runningBots.Remove(id);
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

        var (id, _) = _runningBots.First(x => x.Value.Equals(sender));
        var newEventArgs = new ProcessStdWriteEventArgs(id, eventArgs.Data);
        
        _logWriter.TryAdd(id, eventArgs.Data);
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

        var (id, _) = _runningBots.First(x => x.Value.Equals(sender));
        var newEventArgs = new ProcessStdWriteEventArgs(id, eventArgs.Data);
        
        _logWriter.TryAdd(id, eventArgs.Data);
        OnStderr?.Invoke(this, newEventArgs);
    }
}