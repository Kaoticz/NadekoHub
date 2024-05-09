using System.Text.Json.Serialization;

namespace NadekoHub.Features.Home.Models.Api.Github;

/// <summary>
/// The assets of a Github release.
/// </summary>
/// <param name="Name">The name of the release file.</param>
/// <param name="Url">The url to the release file.</param>
public sealed record Assets(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("browser_download_url")] string Url
);