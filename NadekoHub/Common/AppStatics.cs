using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform.Storage;
using System.Text.RegularExpressions;

namespace NadekoHub.Common;

/// <summary>
/// Defines the application's environment data.
/// </summary>
public static partial class AppStatics
{
    /// <summary>
    /// Defines the default location where the updater configuration and bot instances are stored.
    /// </summary>
#if DEBUG
    public static string AppDefaultConfigDirectoryUri { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "NadekoHubDebug");
#else
    public static string AppDefaultConfigDirectoryUri { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NadekoHub");
#endif

    /// <summary>
    /// Defines the default location where the bot instances are stored.
    /// </summary>
    public static string AppDefaultBotDirectoryUri { get; } = Path.Combine(AppDefaultConfigDirectoryUri, "Bots");

    /// <summary>
    /// Defines the default location where the backups of bot instances are stored.
    /// </summary>
    public static string AppDefaultBotBackupDirectoryUri { get; } = Path.Combine(AppDefaultConfigDirectoryUri, "Backups");

    /// <summary>
    /// Defines the default location where the logs of bot instances are stored.
    /// </summary>
    public static string AppDefaultLogDirectoryUri { get; } = Path.Combine(AppDefaultConfigDirectoryUri, "Logs");

    /// <summary>
    /// Defines the location of the application's configuration file.
    /// </summary>
    public static string AppConfigUri { get; } = Path.Combine(AppDefaultConfigDirectoryUri, "config.json");

    /// <summary>
    /// Defines the location of the application's dependencies.
    /// </summary>
    public static string AppDepsUri { get; } = Path.Combine(AppDefaultConfigDirectoryUri, "Dependencies");

    /// <summary>
    /// Defines a transparent color brush.
    /// </summary>
    public static ImmutableSolidColorBrush TransparentColorBrush { get; } = new(Colors.Transparent);

    /// <summary>
    /// Represents the image formats supported by the views of this application.
    /// </summary>
    public static FilePickerOpenOptions ImageFilePickerOptions { get; } = new()
    {
        AllowMultiple = false,
        FileTypeFilter = new FilePickerFileType[]
        {
            new("Image") { Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.gif", "*.webp" } },
            new("All") { Patterns = new[] { "*.*" } }
        }
    };

    /// <summary>
    /// Matches the version of Ffmpeg from its CLI output.
    /// </summary>
    /// <remarks>Pattern: ^(?:\S+\s+\D*?){2}(git\S+|[\d\.]+)</remarks>
    public static Regex FfmpegVersionRegex { get; } = FfmpegVersionRegexGenerator();

    [GeneratedRegex(@"^(?:\S+\s+\D*?){2}(git\S+|[\d\.]+)", RegexOptions.Compiled)]
    private static partial Regex FfmpegVersionRegexGenerator();
}