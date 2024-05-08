using System.Text.Json.Serialization;

namespace NadekoHub.Features.AppConfig.Models.Api.Evermeet;

/// <summary>
/// Represents a response from the "https://evermeet.cx/ffmpeg/info" endpoint.
/// </summary>
/// <param name="Name">The name of the component.</param>
/// <param name="Type">The type of the component (snapshot or release).</param>
/// <param name="Version">The version of the component.</param>
/// <param name="Size">The size of the component, in bytes.</param>
/// <param name="Download">The download links to the component, where the key is the desired file format.</param>
public sealed record EvermeetInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("size")] uint Size,
    [property: JsonPropertyName("download")] IReadOnlyDictionary<string, EvermeetDownloadInfo> Download
);