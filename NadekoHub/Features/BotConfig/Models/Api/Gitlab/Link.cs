using System.Text.Json.Serialization;

namespace NadekoHub.Features.BotConfig.Models.Api.Gitlab;

/// <summary>
/// The urls of a Gitlab release assets.
/// </summary>
/// <param name="Name">The name of the file.</param>
/// <param name="Url">The url to the asset.</param>
public sealed record Link(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("url")] string Url
);