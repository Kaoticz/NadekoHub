using System.Text.Json.Serialization;

namespace NadekoHub.Features.BotConfig.Models.Api.Gitlab;

/// <summary>
/// The assets of a Gitlab release.
/// </summary>
/// <param name="Links">The urls to the assets.</param>
public sealed record Assets(
    [property: JsonPropertyName("links")] Link[] Links
);