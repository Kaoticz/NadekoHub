using NadekoHub.Features.AppWindow.ViewModels;

namespace NadekoHub.Features.AppWindow.Models;

/// <summary>
/// Represents a bot entry in the <see cref="LateralBarViewModel"/>.
/// </summary>
/// <param name="Id">The Id of the bot.</param>
/// <param name="BotInfo">The information about the bot instance.</param>
public sealed record BotEntry(Guid Id, BotInstanceInfo BotInfo);