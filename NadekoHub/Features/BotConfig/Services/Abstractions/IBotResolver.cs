using NadekoHub.Features.Shared.Services.Abstractions;

namespace NadekoHub.Features.BotConfig.Services.Abstractions;

/// <summary>
/// Represents a service that checks, downloads, installs, and updates a bot instance.
/// </summary>
public interface IBotResolver : IDependencyResolver
{
    /// <summary>
    /// The name of the bot instance.
    /// </summary>
    string BotName { get; }

    /// <summary>
    /// Defines whether there is an ongoing update.
    /// </summary>
    bool IsUpdateInProgress { get; }

    /// <summary>
    /// The Id of the bot.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Creates a backup of the bot instance associated with this resolver.
    /// </summary>
    /// <returns>The absolute path to the backup file or <see langword="null"/> if the backup failed.</returns>
    ValueTask<string?> CreateBackupAsync();
}