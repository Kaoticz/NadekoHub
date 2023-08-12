namespace NadekoUpdater.Common;

/// <summary>
/// Defines constants to be used inside views.
/// </summary>
public static class WindowConstants
{
    /// <summary>
    /// Defines the minimum width of the window.
    /// </summary>
    public const string DefaultWindowWidth = "800";

    /// <summary>
    /// Defines the minimum height of the window.
    /// </summary>
    public const string DefaultWindowHeight = "500";

    /// <summary>
    /// Defines the default window title.
    /// </summary>
    public const string WindowTitle = "NadekoBot Updater";

    /// <summary>
    /// Defines the message that should be shown when a view's parameterless constructor should not be used.
    /// </summary>
    public const string DesignerCtorWarning = "This constructor exists to satisfy Avalonia's designer. Please, use the parameterized constructor instead.";
}