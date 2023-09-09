namespace NadekoHub.Common;

/// <summary>
/// Defines the available types of dialog windows.
/// </summary>
public enum DialogType
{
    /// <summary>
    /// The dialog box notifies the user about something.
    /// </summary>
    Notification,

    /// <summary>
    /// The dialog box notifies the user of a non-fatal error.
    /// </summary>
    Warning,

    /// <summary>
    /// The dialog box notifies the user of a fatal error.
    /// </summary>
    Error
}