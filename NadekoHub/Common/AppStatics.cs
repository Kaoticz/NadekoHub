using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform.Storage;
using System.Reflection;
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
    public static string AppDefaultConfigDirectoryUri { get; } = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "NadekoHubDebug");
#else
    public static string AppDefaultConfigDirectoryUri { get; } = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NadekoHub");
#endif

    /// <summary>
    /// Defines the default location where the bot instances are stored.
    /// </summary>
    public static string AppDefaultBotDirectoryUri { get; } = Path.Join(AppDefaultConfigDirectoryUri, "Bots");

    /// <summary>
    /// Defines the default location where the backups of bot instances are stored.
    /// </summary>
    public static string AppDefaultBotBackupDirectoryUri { get; } = Path.Join(AppDefaultConfigDirectoryUri, "Backups");

    /// <summary>
    /// Defines the default location where the logs of bot instances are stored.
    /// </summary>
    public static string AppDefaultLogDirectoryUri { get; } = Path.Join(AppDefaultConfigDirectoryUri, "Logs");

    /// <summary>
    /// Defines the location of the application's configuration file.
    /// </summary>
    public static string AppConfigUri { get; } = Path.Join(AppDefaultConfigDirectoryUri, "config.json");

    /// <summary>
    /// Defines the location of the application's dependencies.
    /// </summary>
    public static string AppDepsUri { get; } = Path.Join(AppDefaultConfigDirectoryUri, "Dependencies");

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
        FileTypeFilter =
        [
            new("Image") { Patterns = ["*.png", "*.jpg", "*.jpeg", "*.gif", "*.webp"]},
            new("All") { Patterns = ["*.*"]}
        ]
    };

    /// <summary>
    /// The version of this application.
    /// </summary>
    public static string AppVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString()
        ?? throw new InvalidOperationException("Version is missing from application assembly.");

    /// <summary>
    /// Matches the version of Ffmpeg from its CLI output.
    /// </summary>
    /// <remarks>Pattern: ^(?:\S+\s+\D*?){2}(git\S+|[\d\.]+)</remarks>
    public static Regex FfmpegVersionRegex { get; } = FfmpegVersionRegexGenerator();

    [GeneratedRegex(@"^(?:\S+\s+\D*?){2}(git\S+|[\d\.]+)", RegexOptions.Compiled)]
    private static partial Regex FfmpegVersionRegexGenerator();
}