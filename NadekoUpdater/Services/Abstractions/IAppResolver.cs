namespace NadekoUpdater.Services.Abstractions;

/// <summary>
/// Represents a service that updates this application.
/// </summary>
public interface IAppResolver : IDependencyResolver
{
    /// <summary>
    /// The absolute path to the binary file of this application.
    /// </summary>
    string BinaryUri { get; }

    /// <summary>
    /// The suffix appended to the name of old files.
    /// </summary>
    string OldFileSuffix { get; }

    /// <summary>
    /// Removes the files from the old installation.
    /// </summary>
    /// <returns><see langword="true"/> if old files were removed, <see langword="false"/> otherwise.</returns>
    bool RemoveOldFiles();

    /// <summary>
    /// Starts the recently updated version of this application.
    /// </summary>
    void LaunchNewVersion();
}