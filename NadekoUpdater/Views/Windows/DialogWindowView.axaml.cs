using Avalonia.Controls;
using NadekoUpdater.Common;
using NadekoUpdater.ViewModels.Windows;
using System;
using System.Threading.Tasks;

namespace NadekoUpdater.Views.Windows;

/// <summary>
/// Represents a window that presents a message to the user.
/// </summary>
public partial class DialogWindowView : Window
{
    private readonly Window? _parentWindow;

    /// <summary>
    /// Initializes a dialog window.
    /// </summary>
    /// <param name="message">The message to be displayed in the window.</param>
    /// <param name="dialogType">The type of message this dialog is conveying.</param>
    /// <param name="parentWindow">The view that owns this dialog or <see langword="null"/> if there isn't one.</param>
    public DialogWindowView(string message, DialogType dialogType, Window? parentWindow = default)
    {
        InitializeComponent();
        _parentWindow = parentWindow;
        base.DataContext = new DialogWindowViewModel(message, dialogType, this, parentWindow);
    }

    /// <summary>
    /// Shows the window as a dialog.
    /// </summary>
    /// <returns>A task that can be used to track the lifetime of the dialog.</returns>
    /// <exception cref="InvalidOperationException">The window has already been closed.</exception>
    public Task ShowDialog()
    {
        if (_parentWindow is not null)
            return base.ShowDialog(_parentWindow);

        base.Show();
        return Task.CompletedTask;
    }
}