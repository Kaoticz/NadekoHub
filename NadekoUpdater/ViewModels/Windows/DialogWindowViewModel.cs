using Avalonia.Controls;
using NadekoUpdater.Common;
using NadekoUpdater.ViewModels.Abstractions;
using System;

namespace NadekoUpdater.ViewModels.Windows;

/// <summary>
/// View-model for dialog windows.
/// </summary>
public sealed class DialogWindowViewModel : WindowViewModelBase
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
    /// <param name="dialogWindow">The view of the dialog window.</param>
    /// <param name="parentWindow">The view that owns <paramref name="dialogWindow"/> or <see langword="null"/> if there isn't one.</param>
    public DialogWindowViewModel(string message, DialogType dialogType, Window dialogWindow, Window? parentWindow = default)
        : this(message, dialogType.ToString(), dialogWindow, parentWindow)
    {
    }

    /// <summary>
    /// Initializes a dialog window view-model.
    /// </summary>
    /// <param name="message">The message to be displayed in the dialog window.</param>
    /// <param name="title">The title to be displayed in the dialog window.</param>
    /// <param name="dialogWindow">The view of the dialog window.</param>
    /// <param name="parentWindow">The view that owns <paramref name="dialogWindow"/> or <see langword="null"/> if there isn't one.</param>
    public DialogWindowViewModel(string message, string title, Window dialogWindow, Window? parentWindow = default)
        : base(dialogWindow, parentWindow)
    {
        Message = message;
        Title = title;
        CloseWindow = dialogWindow.Close;
    }
}