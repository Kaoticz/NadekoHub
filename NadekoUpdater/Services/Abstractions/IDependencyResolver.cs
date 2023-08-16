namespace NadekoUpdater.Services.Abstractions;

/// <summary>
/// Represents a service that checks, downloads, installs, and updates a dependency.
/// </summary>
public interface IDependencyResolver
{
    /// <summary>
    /// The name of this dependency.
    /// </summary>
    string DependencyName { get; }

    /// <summary>
    /// The name of the dependency binary file.
    /// </summary>
    string FileName { get; }

    /// <summary>
    /// Checks if the dependency can be updated.
    /// </summary>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns>
    /// <see langword="true"/> if the dependency can be updated,
    /// <see langword="false"/> if the dependency is up-to-date,
    /// <see langword="null"/> if the dependency is not installed.
    /// </returns>
    ValueTask<bool?> CanUpdateAsync(CancellationToken cToken = default);

    /// <summary>
    /// Gets the version of the dependency currently installed on this system.
    /// </summary>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns>The version of the dependency on this system or <see langword="null"/> if the dependency is not installed.</returns>
    ValueTask<string?> GetCurrentVersionAsync(CancellationToken cToken = default);

    /// <summary>
    /// Gets the latest version of the dependency.
    /// </summary>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns>The latest version of the dependency.</returns>
    /// <exception cref="InvalidOperationException">
    /// Occurs when there is an issue with the redirection of GitHub's latest release link.
    /// </exception>
    ValueTask<string> GetLatestVersionAsync(CancellationToken cToken = default);

    /// <summary>
    /// Installs or updates the dependency on this system.
    /// </summary>
    /// <param name="dependenciesUri">The absolute path to the directory where the dependency should be installed to.</param>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns>
    /// A tuple that may or may not contain the old and new versions of the dependency. <br />
    /// (<see langword="string"/>, <see langword="null"/>): the dependency is already up-to-date, so no operation was performed. <br />
    /// (<see langword="null"/>, <see langword="string"/>): the dependency got installed. <br />
    /// (<see langword="string"/>, <see langword="string"/>): the dependency got updated.
    /// </returns>
    ValueTask<(string? OldVersion, string? NewVersion)> InstallOrUpdateAsync(string dependenciesUri, CancellationToken cToken = default);
}