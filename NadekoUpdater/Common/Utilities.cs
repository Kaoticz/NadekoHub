using System.ComponentModel;
using System.Diagnostics;

namespace NadekoUpdater.Common;

/// <summary>
/// Miscellaneous utility methods.
/// </summary>
internal static class Utilities
{

    private static readonly string _envPathSeparator = (Environment.OSVersion.Platform is PlatformID.Win32NT) ? ";" : ":";
    private static readonly EnvironmentVariableTarget _envTarget = (Environment.OSVersion.Platform is PlatformID.Win32NT)
        ? EnvironmentVariableTarget.User
        : EnvironmentVariableTarget.Process;

    /// <summary>
    /// Starts the specified program.
    /// </summary>
    /// <param name="program">
    /// The name of the program in the PATH environment variable,
    /// or the absolute path to its executable.
    /// </param>
    /// <param name="arguments">The arguments to the program.</param>
    /// <returns>The process of the specified program.</returns>
    /// <exception cref="Win32Exception">Occurs when <paramref name="program"/> does not exist.</exception>
    public static Process StartProcess(string program, string arguments = "")
    {
        return Process.Start(new ProcessStartInfo()
        {
            FileName = program,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
        })!;
    }

    /// <summary>
    /// Checks if a program exists.
    /// </summary>
    /// <param name="programName">The name of the program.</param>
    /// <returns><see langword="true"/> if the program exists, <see langword="false"/> otherwise.</returns>
    public static async ValueTask<bool> ProgramExistsAsync(string programName)
    {
        using var process = StartProcess((Environment.OSVersion.Platform is PlatformID.Win32NT) ? "where" : "which", programName);
        return !string.IsNullOrWhiteSpace(await process.StandardOutput.ReadToEndAsync());
    }

    /// <summary>
    /// Adds a directory path to the PATH environment variable.
    /// </summary>
    /// <param name="directoryPath">The absolute path to a directory.</param>
    /// <returns><see langword="true"/> if path got successfully added to the PATH envar, <see langword="false"/> otherwise.</returns>
    public static bool AddPathToPATHEnvar(string directoryPath)
    {
        var envPathValue = Environment.GetEnvironmentVariable("PATH", _envTarget) ?? string.Empty;

        if (envPathValue.Contains(directoryPath, StringComparison.Ordinal))
            return false;

        var newPathEnvValue = envPathValue + _envPathSeparator + directoryPath;

        // Add path to Windows' user envar, so it persists across reboots.
        if (Environment.OSVersion.Platform is PlatformID.Win32NT)
            Environment.SetEnvironmentVariable("PATH", newPathEnvValue, EnvironmentVariableTarget.User);

        // Add path to the current process' envar, so the updater can see the dependencies.
        Environment.SetEnvironmentVariable("PATH", newPathEnvValue, EnvironmentVariableTarget.Process);

        return true;
    }
}