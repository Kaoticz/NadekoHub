namespace NadekoHub.Enums;

/// <summary>
/// Defines the possible status for a dependency.
/// </summary>
public enum DependencyStatus
{
    /// <summary>
    /// The dependency is available for installation.
    /// </summary>
    Install,

    /// <summary>
    /// The dependency is installed and up-to-date.
    /// </summary>
    Installed,

    /// <summary>
    /// The dependency is in the process of being updated.
    /// </summary>
    Updating,

    /// <summary>
    /// The dependency has an update available.
    /// </summary>
    Update,

    /// <summary>
    /// The dependency is currently being checked for updates.
    /// </summary>
    Checking
}