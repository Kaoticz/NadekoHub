using Avalonia.Platform;
using SkiaSharp;
using System.ComponentModel;
using System.Diagnostics;

namespace NadekoHub.Common;

/// <summary>
/// Miscellaneous utility methods.
/// </summary>
internal static class Utilities
{
    /// <summary>
    /// Loads an image embeded with this application.
    /// </summary>
    /// <param name="uri">An uri that starts with "avares://"</param>
    /// <remarks>Valid uris must start with "avares://".</remarks>
    /// <returns>The embeded image or the default bot avatar placeholder.</returns>
    /// <exception cref="FileNotFoundException">Occurs when the embeded resource does not exist.</exception>
    public static SKBitmap LoadEmbededImage(string? uri = default)
    {
        return (string.IsNullOrWhiteSpace(uri) || !uri.StartsWith("avares://", StringComparison.Ordinal))
            ? SKBitmap.Decode(AssetLoader.Open(new Uri(AppConstants.BotAvatarUri)))
            : SKBitmap.Decode(AssetLoader.Open(new Uri(uri)));
    }

    /// <summary>
    /// Loads the image at the specified location or the bot avatar placeholder if it was not found.
    /// </summary>
    /// <param name="uri">The absolute path to the image file or <see langword="null"/> to get the avatar placeholder.</param>
    /// <remarks>This fallsback to <see cref="LoadEmbededImage(string?)"/> if <paramref name="uri"/> doesn't point to a valid image file.</remarks>
    /// <returns>The requested image or the default bot avatar placeholder.</returns>
    public static SKBitmap LoadLocalImage(string? uri = default)
    {
        return (File.Exists(uri))
            ? SKBitmap.Decode(uri)
            : LoadEmbededImage(uri);
    }
}