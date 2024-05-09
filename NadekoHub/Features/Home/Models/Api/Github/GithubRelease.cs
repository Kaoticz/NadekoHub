using System.Text.Json.Serialization;

namespace NadekoHub.Features.Home.Models.Api.Github;

/// <summary>
/// Represents a release from the Github API.
/// </summary>
/// <param name="Tag">The tag of the release.</param>
/// <param name="Assets">The assets of the release.</param>
public sealed record GithubRelease(
    [property: JsonPropertyName("tag_name")] string Tag,
    [property: JsonPropertyName("assets")] Assets[] Assets
);
