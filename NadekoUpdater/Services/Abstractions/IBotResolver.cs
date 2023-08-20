namespace NadekoUpdater.Services.Abstractions;

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
    /// The position of the bot instance on the lateral bar.
    /// </summary>
    uint Position { get; }

    /// <summary>
    /// Creates a backup of the bot instance associated with this resolver.
    /// </summary>
    /// <returns>The absolute path to the backup file or <see langword="null"/> if the backup failed.</returns>
    string? CreateBackup();
}