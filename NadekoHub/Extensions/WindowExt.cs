using Avalonia.Controls;
using Avalonia.Styling;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

namespace NadekoHub.Extensions;

/// <summary>
/// Provides extension methods for <see cref="Window"/>.
/// </summary>
public static class WindowExt
{
    /// <summary>
    /// Shows a dialog window that blocks the main window.
    /// </summary>
    /// <param name="activeView">The active window.</param>
    /// <param name="message">The message to be displayed.</param>
    /// <param name="dialogType">The type of dialog window to display.</param>
    /// <param name="iconType">The icon to be displayed.</param>
    /// <returns>The button type that was pressed.</returns>
    public static Task<ButtonResult> ShowDialogWindowAsync(this Window activeView, string message, DialogType dialogType = DialogType.Notification, Icon iconType = Icon.None)
        => ShowDialogWindowAsync(activeView, message, dialogType.ToString(), iconType);

    /// <summary>
    /// Shows a dialog window that blocks the main window.
    /// </summary>
    /// <param name="activeView">The active window.</param>
    /// <param name="message">The message to be displayed.</param>
    /// <param name="title">The title of the dialog box.</param>
    /// <param name="iconType">The icon to be displayed.</param>
    /// <returns>The button type that was pressed.</returns>
    public static Task<ButtonResult> ShowDialogWindowAsync(this Window activeView, string message, string title, Icon iconType = Icon.None)
    {
        var messageparameters = new MessageBoxStandardParams()
        {
            ButtonDefinitions = ButtonEnum.Ok,
            ContentMessage = message,
            ContentTitle = title,
            Icon = iconType,
            WindowIcon = activeView.GetResource<WindowIcon>(AppResources.NadekoHubIcon),
            MaxWidth = int.Parse(WindowConstants.DefaultWindowWidth) / 1.7,
            SizeToContent = SizeToContent.WidthAndHeight,
            ShowInCenter = true,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        return ShowDialogWindowAsync(activeView, messageparameters);
    }

    /// <summary>
    /// Shows a dialog window that blocks the main window.
    /// </summary>
    /// <param name="activeView">The active window.</param>
    /// <param name="messageParameters">The parameters of the message dialog box.</param>
    /// <returns>The button type that was pressed.</returns>
    public static Task<ButtonResult> ShowDialogWindowAsync(this Window activeView, MessageBoxStandardParams messageParameters)
        => MessageBoxManager.GetMessageBoxStandard(messageParameters).ShowWindowDialogAsync(activeView);

    /// <summary>
    /// Finds the specified resource by searching up the logical tree and then global styles.
    /// </summary>
    /// <typeparam name="T">The type of the resource.</typeparam>
    /// <param name="activeView">The active window.</param>
    /// <param name="resourceName">The name of the resource.</param>
    /// <returns>The requested <typeparamref name="T"/> resource.</returns>
    /// <exception cref="InvalidOperationException">Occurs when the resource is not found.</exception>
    /// <exception cref="InvalidCastException">Occurs when the resource is not of type <typeparamref name="T"/>.</exception>
    public static T GetResource<T>(this Window activeView, string resourceName)
        => GetResource<T>(activeView, resourceName, activeView.ActualThemeVariant);

    /// <summary>
    /// Finds the specified resource by searching up the logical tree and then global styles.
    /// </summary>
    /// <typeparam name="T">The type of the resource.</typeparam>
    /// <param name="activeView">The active window.</param>
    /// <param name="resourceName">The name of the resource.</param>
    /// <param name="theme">The UI theme variant the resource belongs to.</param>
    /// <returns>The requested <typeparamref name="T"/> resource.</returns>
    /// <exception cref="InvalidOperationException">Occurs when the resource is not found.</exception>
    /// <exception cref="InvalidCastException">Occurs when the resource is not of type <typeparamref name="T"/>.</exception>
    public static T GetResource<T>(this Window activeView, string resourceName, ThemeVariant theme)
    {
        return (!activeView.TryFindResource(resourceName, theme, out var resource))
            ? throw new InvalidOperationException($"Resource '{resourceName}' was not found.")
            : (!Utilities.TryCastTo<T>(resource, out var result))
                ? throw new InvalidCastException($"Could not convert resource of type '{resource?.GetType()?.FullName}' to '{nameof(T)}'.")
                : result;
    }
}