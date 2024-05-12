namespace NadekoHub.Features.AppWindow.Models;

/// <summary>
/// Represents a bot entry from NadekoUpdater.
/// </summary>
/// <param name="Guid">The Id of the bot.</param>
/// <param name="Name">The name of the bot.</param>
/// <param name="IconUri">The path to the avatar image.</param>
/// <param name="Version">The version of the bot.</param>
/// <param name="PathUri">The path to the bot files.</param>
internal sealed record OldUpdaterBotEntry(Guid Guid, string Name, string? IconUri, string? Version, string? PathUri);