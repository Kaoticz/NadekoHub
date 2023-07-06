using NadekoUpdater.ViewModels.Controls;

namespace NadekoUpdater.Models;

/// <summary>
/// Represents a bot entry in the <see cref="LateralBarViewModel"/>.
/// </summary>
/// <param name="Position">The position of the entry in the lateral bar.</param>
/// <param name="BotInfo">The information about the bot instance.</param>
public sealed record BotEntry(uint Position, BotInstanceInfo BotInfo);