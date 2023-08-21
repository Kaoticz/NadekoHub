using Avalonia.Controls;
using Avalonia.Platform;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace NadekoUpdater.Extensions;

/// <summary>
/// Provides extension methods for <see cref="Window"/>.
/// </summary>
public static class WindowExt
{
    private static readonly WindowIcon _dialogWindowIcon = new(AssetLoader.Open(new Uri(AppStatics.ApplicationWindowIcon)));

    /// <summary>
    /// Shows a dialog window that blocks the main window.
    /// </summary>
    /// <param name="activeView">The active window.</param>
    /// <param name="message">The message to be displayed.</param>
    /// <param name="dialogType">The type of dialog window to display.</param>
    /// <param name="iconType">The icon to be displayed.</param>
    public static Task ShowDialogWindowAsync(this Window activeView, string message, DialogType dialogType = DialogType.Notification, Icon iconType = Icon.None)
        => ShowDialogWindowAsync(activeView, message, dialogType.ToString(), iconType);

    /// <summary>
    /// Shows a dialog window that blocks the main window.
    /// </summary>
    /// <param name="activeView">The active window.</param>
    /// <param name="message">The message to be displayed.</param>
    /// <param name="title">The title of the dialog box.</param>
    /// <param name="iconType">The icon to be displayed.</param>
    public static Task ShowDialogWindowAsync(this Window activeView, string message, string title, Icon iconType = Icon.None)
    {
        var dialogBox = MessageBoxManager.GetMessageBoxStandard(new()
        {
            ButtonDefinitions = ButtonEnum.Ok,
            ContentMessage = message,
            ContentTitle = title,
            Icon = iconType,
            WindowIcon = _dialogWindowIcon,
            MaxWidth = int.Parse(WindowConstants.DefaultWindowWidth) / 1.7,
            SizeToContent = SizeToContent.WidthAndHeight,
            ShowInCenter = true,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
        });

        return dialogBox.ShowWindowDialogAsync(activeView);
    }
}