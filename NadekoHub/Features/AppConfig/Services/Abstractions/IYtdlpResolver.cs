using NadekoHub.Features.Common.Services.Abstractions;

namespace NadekoHub.Features.AppConfig.Services.Abstractions;

/// <summary>
/// Represents a service that checks, downloads, installs, and updates yt-dlp.
/// </summary>
/// <remarks>This interface exists mainly for DI registration.</remarks>
public interface IYtdlpResolver : IDependencyResolver
{
}