using Avalonia.Controls;
using NadekoUpdater.Common;
using NadekoUpdater.ViewModels;

namespace NadekoUpdater.Views;

/// <summary>
/// Represents a window that presents a message to the user.
/// </summary>
public partial class DialogWindowView : Window
{
    /// <summary>
    /// Initializes a dialog window.
    /// </summary>
    /// <param name="message">The message to be displayed in the window.</param>
    public DialogWindowView(string message)
    {
        InitializeComponent();
        base.DataContext = new DialogWindowViewModel(message, DialogType.Notification, this);
    }
}