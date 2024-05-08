using System.Text.Json.Serialization;

namespace NadekoHub.Features.BotConfig.Models.Api.Gitlab;

/// <summary>
/// Represents a release from the Gitlab API.
/// </summary>
/// <param name="Tag">The tag of the release.</param>
/// <param name="Assets">The assets of the release.</param>
public sealed record GitlabRelease(
    [property: JsonPropertyName("tag_name")] string Tag,
    [property: JsonPropertyName("assets")] Assets Assets
);