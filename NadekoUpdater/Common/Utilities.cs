using System.ComponentModel;
using System.Diagnostics;

namespace NadekoUpdater.Common;

/// <summary>
/// Miscellaneous utility methods.
/// </summary>
internal static class Utilities
{
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
        if (Environment.GetEnvironmentVariable("PATH")?.Contains(programName, StringComparison.Ordinal) is true)
            return true;

        using var process = StartProcess((Environment.OSVersion.Platform is PlatformID.Win32NT) ? "where" : "which", programName);

        return !string.IsNullOrWhiteSpace(await process.StandardOutput.ReadToEndAsync());
    }
}