using NadekoHub.Features.AppConfig.Models;
using NadekoHub.Features.BotConfig.Models;
using NadekoHub.Features.BotConfig.Services.Abstractions;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace NadekoHub.Features.BotConfig.Services;

/// <summary>
/// Defines a service that writes logs of bot instances to the disk.
/// </summary>
public sealed class LogWriter : ILogWriter
{
    private readonly Dictionary<Guid, StringBuilder> _botLogs = new();
    private readonly ReadOnlyAppSettings _appConfig;

    /// <inheritdoc/>
    public event EventHandler<ILogWriter, LogFlushEventArgs>? OnLogCreated;

    /// <summary>
    /// Creates a service that writes logs of bot instances to the disk.
    /// </summary>
    /// <param name="appConfig">The application settings.</param>
    public LogWriter(ReadOnlyAppSettings appConfig)
        => _appConfig = appConfig;

    /// <inheritdoc/>
    public async Task<bool> FlushAllAsync(bool removeFromMemory = false, CancellationToken cToken = default)
    {
        var result = await Task.WhenAll(_botLogs.Keys.Select(x => FlushAsync(x, removeFromMemory, cToken)));
        return result.Any(x => x);
    }

    /// <inheritdoc/>
    public async Task<bool> FlushAsync(Guid botId, bool removeFromMemory = false, CancellationToken cToken = default)
    {
        if (!_botLogs.TryGetValue(botId, out var logStringBuilder))
            return false;

        if (removeFromMemory)
            _botLogs.Remove(botId);

        if (logStringBuilder.Length is 0)
            return false;

        Directory.CreateDirectory(_appConfig.LogsDirectoryUri);
        var logByteSize = logStringBuilder.Length * 2 + 1;
        var botEntry = _appConfig.BotEntries[botId];
        var now = DateTimeOffset.Now;
        var date = new DateOnly(now.Year, now.Month, now.Day).ToShortDateString().Replace('/', '-');
        var fileUri = Path.Combine(_appConfig.LogsDirectoryUri, $"{botEntry.Name}_v{botEntry.Version}_{date}-{now.ToUnixTimeSeconds()}.txt");

        await File.WriteAllTextAsync(fileUri, logStringBuilder.ToString(), cToken);

        logStringBuilder.Clear();

        OnLogCreated?.Invoke(this, new(fileUri, logByteSize, now));

        return true;
    }

    /// <inheritdoc/>
    public bool TryAdd(Guid botId, string message)
    {
        if (string.IsNullOrWhiteSpace(message) || _appConfig.LogMaxSizeMb <= 0.0)
            return false;

        if (!_botLogs.TryGetValue(botId, out var logStringBuilder))
        {
            logStringBuilder = new();
            _botLogs.TryAdd(botId, logStringBuilder);
        }

        logStringBuilder.AppendLine(message);

        if (logStringBuilder.Length > _appConfig.LogMaxSizeMb * 1_000_000)
            _ = FlushAsync(botId);

        return true;
    }

    /// <inheritdoc/>
    public bool TryRead(Guid botId, [MaybeNullWhen(false)] out string log)
    {
        if (_botLogs.TryGetValue(botId, out var logStringBuilder))
        {
            log = logStringBuilder.ToString();
            return true;
        }

        log = null;
        return false;
    }
}