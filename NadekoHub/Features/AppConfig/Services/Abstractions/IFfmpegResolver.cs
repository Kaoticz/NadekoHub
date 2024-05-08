using NadekoHub.Features.Shared.Services.Abstractions;

namespace NadekoHub.Features.AppConfig.Services.Abstractions;

/// <summary>
/// Represents a service that checks, downloads, installs, and updates ffmpeg.
/// </summary>
/// <remarks>This interface exists mainly for DI registration.</remarks>
public interface IFfmpegResolver : IDependencyResolver
{
}