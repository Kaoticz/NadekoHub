using Avalonia.Controls;
using NadekoUpdater.Common;
using System;

namespace NadekoUpdater.ViewModels;

/// <summary>
/// View-model for dialog windows.
/// </summary>
public sealed class DialogWindowViewModel : ViewModelBase
{
    /// <summary>
    /// The title of the dialog window.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// The message to be displayed in the dialog window.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Action that closes the dialog window.
    /// </summary>
    public Action CloseWindow { get; }

    /// <summary>
    /// Initializes a dialog window view-model.
    /// </summary>
    /// <param name="message">The message to be displayed in the dialog window.</param>
    /// <param name="dialogType">The type of message this dialog is conveying.</param>
    /// <param name="dialogWindow">The dialog window itself.</param>
    public DialogWindowViewModel(string message, DialogType dialogType, Window dialogWindow) : this(message, dialogType.ToString(), dialogWindow)
    {
    }

    /// <summary>
    /// Initializes a dialog window view-model.
    /// </summary>
    /// <param name="message">The message to be displayed in the dialog window.</param>
    /// <param name="title">The title to be displayed in the dialog window.</param>
    /// <param name="dialogWindow">The dialog window itself.</param>
    public DialogWindowViewModel(string message, string title, Window dialogWindow)
    {
        Message = message;
        Title = title;
        CloseWindow = dialogWindow.Close;
    }
}