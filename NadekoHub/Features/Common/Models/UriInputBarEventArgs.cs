using NadekoHub.Features.Common.ViewModels;

namespace NadekoHub.Features.Common.Models;

/// <summary>
/// Defines the event arguments for when a valid uri is set to a <see cref="UriInputBarViewModel"/>.
/// </summary>
public sealed class UriInputBarEventArgs : EventArgs
{
    /// <summary>
    /// The old valid uri.
    /// </summary>
    public string OldUri { get; }

    /// <summary>
    /// The new valid uri.
    /// </summary>
    public string NewUri { get; }

    /// <summary>
    /// Creates the event arguments for when a valid uri is set to a <see cref="UriInputBarViewModel"/>.
    /// </summary>
    /// <param name="oldUri">The old valid uri.</param>
    /// <param name="newUri">The new valid uri.</param>
    public UriInputBarEventArgs(string oldUri, string newUri)
    {
        OldUri = oldUri;
        NewUri = newUri;
    }
}