using Kotz.Events;
using NadekoUpdater.Models.Config;
using NadekoUpdater.Models.EventArguments;
using NadekoUpdater.Services.Abstractions;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace NadekoUpdater.Services;

/// <summary>
/// Defines a service that writes logs of bot instances to the disk.
/// </summary>
public sealed class LogWriter : ILogWriter
{
    private readonly Dictionary<uint, StringBuilder> _botLogs = new();
    private readonly ReadOnlyAppConfig _appConfig;

    /// <inheritdoc/>
    public event EventHandler<ILogWriter, LogFlushEventArgs>? OnLogCreated;

    /// <summary>
    /// Creates a service that writes logs of bot instances to the disk.
    /// </summary>
    /// <param name="appConfig">The application settings.</param>
    public LogWriter(ReadOnlyAppConfig appConfig)
        => _appConfig = appConfig;

    /// <inheritdoc/>
    public async Task<bool> FlushAllAsync(bool removeFromMemory = false, CancellationToken cToken = default)
    {
        var result = await Task.WhenAll(_botLogs.Keys.Select(x => FlushAsync(x, removeFromMemory, cToken)));
        return result.Any(x => x);
    }

    /// <inheritdoc/>
    public async Task<bool> FlushAsync(uint botPosition, bool removeFromMemory = false, CancellationToken cToken = default)
    {
        if (!_botLogs.TryGetValue(botPosition, out var logStringBuilder))
            return false;

        if (removeFromMemory)
            _botLogs.Remove(botPosition);

        if (logStringBuilder.Length is 0)
            return false;

        Directory.CreateDirectory(_appConfig.LogsDirectoryUri);
        var logByteSize = (logStringBuilder.Length * 2) + 1;
        var botEntry = _appConfig.BotEntries[botPosition];
        var now = DateTimeOffset.Now;
        var date = new DateOnly(now.Year, now.Month, now.Day).ToShortDateString().Replace('/', '-');
        var fileUri = Path.Combine(_appConfig.LogsDirectoryUri, $"{botEntry.Name}_{date}-{now.ToUnixTimeSeconds()}.txt");

        await File.WriteAllTextAsync(fileUri, logStringBuilder.ToString(), cToken);

        logStringBuilder.Clear();

        OnLogCreated?.Invoke(this, new(fileUri, logByteSize, now));

        return true;
    }

    /// <inheritdoc/>
    public bool TryAdd(uint botPosition, string message)
    {
        if (string.IsNullOrWhiteSpace(message) || _appConfig.LogMaxSizeMb <= 0.0)
            return false;

        if (!_botLogs.TryGetValue(botPosition, out var logStringBuilder))
        {
            logStringBuilder = new();
            _botLogs.TryAdd(botPosition, logStringBuilder);
        }

        logStringBuilder.AppendLine(message);

        if ((logStringBuilder.Length > _appConfig.LogMaxSizeMb * 1_000_000))
            _ = FlushAsync(botPosition);

        return true;
    }

    /// <inheritdoc/>
    public bool TryRead(uint botPosition, [MaybeNullWhen(false)] out string log)
    {
        if (_botLogs.TryGetValue(botPosition, out var logStringBuilder))
        {
            log = logStringBuilder.ToString();
            return true;
        }

        log = null;
        return false;
    }
}