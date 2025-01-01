using Avalonia.Platform;
using SkiaSharp;

namespace NadekoHub.Common;

/// <summary>
/// Miscellaneous utility methods.
/// </summary>
internal static class Utilities
{
    /// <summary>
    /// Loads an image embedded with this application.
    /// </summary>
    /// <param name="uri">An uri that starts with "avares://"</param>
    /// <remarks>Valid uris must start with "avares://".</remarks>
    /// <returns>The embedded image or the default bot avatar placeholder.</returns>
    /// <exception cref="FileNotFoundException">Occurs when the embedded resource does not exist.</exception>
    public static SKBitmap LoadEmbeddedImage(string? uri = default)
    {
        return (string.IsNullOrWhiteSpace(uri) || !uri.StartsWith("avares://", StringComparison.Ordinal))
            ? SKBitmap.Decode(AssetLoader.Open(new(AppConstants.BotAvatarUri)))
            : SKBitmap.Decode(AssetLoader.Open(new(uri)));
    }

    /// <summary>
    /// Loads the image at the specified location or the bot avatar placeholder if it was not found.
    /// </summary>
    /// <param name="imagePath">The absolute path to the image file or <see langword="null"/> to get the avatar placeholder.</param>
    /// <remarks>This fallsback to <see cref="LoadEmbeddedImage"/> if <paramref name="imagePath"/> doesn't point to a valid image file.</remarks>
    /// <returns>The requested image or the default bot avatar placeholder.</returns>
    public static SKBitmap LoadLocalImage(string? imagePath)
    {
        return (File.Exists(imagePath))
            ? SKBitmap.Decode(imagePath)
            : LoadEmbeddedImage(imagePath);
    }
}