namespace NadekoUpdater.Models;

/// <summary>
/// Represents the information of a bot instance.
/// </summary>
/// <param name="Name">The name of the bot.</param>
/// <param name="InstanceDirectoryUri">The path to the directory where the bot instance is located at.</param>
/// <param name="Version">The version of the bot or <see langword="null"/> if the bot hasn't been downloaded yet.</param>
/// <param name="AvatarUri">The path to the bot's avatar image file or <see langword="null"/> is there is none.</param>
public sealed record BotInstanceInfo(string Name, string InstanceDirectoryUri, string? Version = default, string? AvatarUri = default);