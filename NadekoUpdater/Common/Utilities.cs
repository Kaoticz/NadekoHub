using Avalonia.Platform;
using SkiaSharp;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace NadekoUpdater.Common;

/// <summary>
/// Miscellaneous utility methods.
/// </summary>
internal static class Utilities
{
    private static readonly string _programVerifier = (Environment.OSVersion.Platform is PlatformID.Win32NT) ? "where" : "which";
    private static readonly string _envPathSeparator = (Environment.OSVersion.Platform is PlatformID.Win32NT) ? ";" : ":";
    private static readonly EnvironmentVariableTarget _envTarget = (Environment.OSVersion.Platform is PlatformID.Win32NT)
        ? EnvironmentVariableTarget.User
        : EnvironmentVariableTarget.Process;

    /// <summary>
    /// Loads an image embeded with this application.
    /// </summary>
    /// <param name="uri">An uri that starts with "avares://"</param>
    /// <remarks>Valid uris must start with "avares://".</remarks>
    /// <returns>The embeded image or the default bot avatar placeholder.</returns>
    /// <exception cref="FileNotFoundException">Occurs when the embeded resource does not exist.</exception>
    public static SKBitmap LoadEmbededImage(string? uri = default)
    {
        return (string.IsNullOrWhiteSpace(uri) || !uri.StartsWith("avares://"))
            ? SKBitmap.Decode(AssetLoader.Open(new Uri(AppConstants.BotAvatarUri)))
            : SKBitmap.Decode(AssetLoader.Open(new Uri(uri)));
    }

    /// <summary>
    /// Loads the image at the specified location or the bot avatar placeholder if it was not found.
    /// </summary>
    /// <param name="uri">The absolute path to the image file or <see langword="null"/> to get the avatar placeholder.</param>
    /// <remarks>This fallsback to <see cref="LoadEmbededImage(string?)"/> if <paramref name="uri"/> doesn't point to a valid image file.</remarks>
    /// <returns>The requested image or the default bot avatar placeholder.</returns>
    public static SKBitmap LoadLocalImage(string? uri = default)
    {
        return (File.Exists(uri))
            ? SKBitmap.Decode(uri)
            : LoadEmbededImage(uri);
    }

    /// <summary>
    /// Safely casts an <see cref="object"/> to a <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to cast to.</typeparam>
    /// <param name="obj">The object to be cast.</param>
    /// <param name="castObject">The cast object, or <see langword="null"/> is casting failed.</param>
    /// <returns><see langword="true"/> if the object was successfully cast, <see langword="false"/> otherwise.</returns>
    public static bool TryCastTo<T>(object? obj, [MaybeNullWhen(false)] out T castObject)
    {
        if (obj is T result)
        {
            castObject = result;
            return true;
        }

        castObject = default;
        return false;
    }

    /// <summary>
    /// Starts the specified program in the background.
    /// </summary>
    /// <param name="program">
    /// The name of the program in the PATH environment variable,
    /// or the absolute path to its executable.
    /// </param>
    /// <param name="arguments">The arguments to the program.</param>
    /// <returns>The process of the specified program.</returns>
    /// <exception cref="ArgumentException" />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="Win32Exception">Occurs when <paramref name="program"/> does not exist.</exception>
    /// <exception cref="InvalidOperationException">Occurs when the process fails to execute.</exception>
    public static Process StartProcess(string program, string arguments = "")
    {
        ArgumentException.ThrowIfNullOrEmpty(program, nameof(program));
        ArgumentNullException.ThrowIfNull(arguments, nameof(arguments));

        return Process.Start(new ProcessStartInfo()
        {
            FileName = program,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
        }) ?? throw new InvalidOperationException($"Failed spawing process for: {program} {arguments}");
    }

    /// <summary>
    /// Checks if a program exists.
    /// </summary>
    /// <param name="programName">The name of the program.</param>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if the program exists, <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentException" />
    /// <exception cref="ArgumentNullException" />
    public static async ValueTask<bool> ProgramExistsAsync(string programName, CancellationToken cToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(programName, nameof(programName));

        using var process = StartProcess(_programVerifier, programName);
        return !string.IsNullOrWhiteSpace(await process.StandardOutput.ReadToEndAsync(cToken));
    }

    /// <summary>
    /// Safely deletes a file.
    /// </summary>
    /// <param name="fileUri">The absolute path to the file.</param>
    /// <returns><see langword="true"/> if the file was deleted, <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentException" />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="IOException" />
    /// <exception cref="NotSupportedException" />
    /// <exception cref="PathTooLongException" />
    /// <exception cref="UnauthorizedAccessException" />
    public static bool TryDeleteFile(string fileUri)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileUri, nameof(fileUri));

        if (!File.Exists(fileUri))
            return false;

        File.Delete(fileUri);
        return true;
    }

    /// <summary>
    /// Safely deletes a directory.
    /// </summary>
    /// <param name="directoryUri">The absolute path to the directory.</param>
    /// <returns><see langword="true"/> if the directory was deleted, <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentException" />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="IOException" />
    /// <exception cref="DirectoryNotFoundException" />
    /// <exception cref="PathTooLongException" />
    /// <exception cref="UnauthorizedAccessException" />
    public static bool TryDeleteDirectory(string directoryUri)
    {
        ArgumentException.ThrowIfNullOrEmpty(directoryUri, nameof(directoryUri));

        if (!Directory.Exists(directoryUri))
            return false;

        Directory.Delete(directoryUri, true);
        return true;
    }

    /// <summary>
    /// Adds a directory path to the PATH environment variable.
    /// </summary>
    /// <param name="directoryUri">The absolute path to a directory.</param>
    /// <remarks>
    /// On Windows, this needs to be called once and the dependencies will be available for the user forever. <br />
    /// On Unix systems, we can only add to the PATH on a process basis, so this needs to be called at least once everytime the application is opened.
    /// </remarks>
    /// <returns><see langword="true"/> if <paramref name="directoryUri"/> got successfully added to the PATH envar, <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentException" />
    /// <exception cref="ArgumentNullException" />
    public static bool AddPathToPATHEnvar(string directoryUri)
    {
        ArgumentException.ThrowIfNullOrEmpty(directoryUri, nameof(directoryUri));

        if (File.Exists(directoryUri))
            throw new ArgumentException("Parameter must point to a directory, not a file.", nameof(directoryUri));

        var envPathValue = Environment.GetEnvironmentVariable("PATH", _envTarget) ?? string.Empty;

        // If directoryPath is already in the PATH envar, don't add it again.
        if (envPathValue.Contains(directoryUri, StringComparison.Ordinal))
            return false;

        var newPathEnvValue = envPathValue + _envPathSeparator + directoryUri;

        // Add path to Windows' user envar, so it persists across reboots.
        if (Environment.OSVersion.Platform is PlatformID.Win32NT)
            Environment.SetEnvironmentVariable("PATH", newPathEnvValue, EnvironmentVariableTarget.User);

        // Add path to the current process' envar, so the updater can see the dependencies.
        Environment.SetEnvironmentVariable("PATH", newPathEnvValue, EnvironmentVariableTarget.Process);

        return true;
    }
}