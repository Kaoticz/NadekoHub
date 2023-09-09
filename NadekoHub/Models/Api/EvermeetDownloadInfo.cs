using System.Text.Json.Serialization;

namespace NadekoHub.Models.Api;

/// <summary>
/// Represents download information for a <see cref="EvermeetInfo"/> component.
/// </summary>
/// <param name="Url">The url to download the component.</param>
/// <param name="Size">The size of the package to download, in bytes.</param>
/// <param name="SignatureUrl">The url to download the cryptographic signature of the component.</param>
public sealed record EvermeetDownloadInfo(
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("size")] uint Size,
    [property: JsonPropertyName("sig")] string SignatureUrl
);