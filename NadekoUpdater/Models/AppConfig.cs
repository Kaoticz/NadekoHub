using System.Collections.Concurrent;

namespace NadekoUpdater.Models;

/// <summary>
/// Represents the settings of the application.
/// </summary>
public sealed record AppConfig
{
    /// <summary>
    /// The absolute path to the directory where the bot instances will be stored.
    /// </summary>
    public string BotsDirectoryUri { get; set; }

    /// <summary>
    /// A collection of metadata about the bot instances.
    /// </summary>
    public ConcurrentDictionary<uint, BotInstanceInfo> BotEntries { get; init; }

    /// <summary>
    /// Creates the settings of the application.
    /// </summary>
    /// <param name="botsDirectoryUri">The absolute path to the directory where the bot instances will be stored.</param>
    /// <param name="botEntries">A collection of metadata about the bot instances.</param>
    public AppConfig(string botsDirectoryUri, ConcurrentDictionary<uint, BotInstanceInfo> botEntries)
    {
        BotsDirectoryUri = botsDirectoryUri;
        BotEntries = botEntries;
    }
}