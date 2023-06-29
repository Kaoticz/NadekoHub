using Avalonia.Controls;
using NadekoUpdater.Common;
using NadekoUpdater.ViewModels.Abstractions;
using NadekoUpdater.Views.Controls;
using NadekoUpdater.Views.Windows;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace NadekoUpdater.ViewModels.Controls;

/// <summary>
/// View-model for <see cref="LateralBarView"/>, the lateral bar with home, bot, and configuration buttons.
/// </summary>
public sealed class LateralBarViewModel : ControlViewModelBase
{
    /// <summary>
    /// Collection of buttons for bot instances.
    /// </summary>
    public ObservableCollection<Button> BotButtonList { get; } = new();

    /// <summary>
    /// Creates a view-model for <see cref="LateralBarView"/>.
    /// </summary>
    /// <param name="control">The <see cref="LateralBarView"/> itself.</param>
    /// <param name="window">The view the <paramref name="control"/> is being rendered into.</param>
    public LateralBarViewModel(UserControl control, Window window) : base(control, window)
    {
    }

    /// <summary>
    /// Adds a new bot button to the lateral bar.
    /// </summary>
    public void AddBotButton()
    {
        BotButtonList.Add(new() { Content = "Bot" });
        this.RaisePropertyChanged(nameof(BotButtonList));
    }

    /// <summary>
    /// Shows a dialog window. Used for debug purposes. To be removed.
    /// </summary>
    /// <param name="message">The message contained in the dialog window.</param>
    public void ShowMessage(string message)
    {
        var window = new DialogWindowView(message, DialogType.Notification, base.View);
        window.ShowDialog();
    }
}