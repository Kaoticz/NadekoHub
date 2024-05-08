namespace NadekoHub.Features.AppConfig.Models;

/// <summary>
/// Represents the dimensions of the application's window.
/// </summary>
/// <param name="Width">The width of the application window.</param>
/// <param name="Height">The height of the application window.</param>
public sealed record WindowSize(double Width, double Height);