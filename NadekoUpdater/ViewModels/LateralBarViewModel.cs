using Avalonia;
using Avalonia.Controls;
using NadekoUpdater.Views;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace NadekoUpdater.ViewModels;

/// <summary>
/// View-model for the lateral bar with home, bot, and configuration buttons.
/// </summary>
public sealed class LateralBarViewModel : ViewModelBase
{
    /// <summary>
    /// Collection of buttons for bot instances.
    /// </summary>
    public ObservableCollection<Button> BotButtonList { get; } = new();

    /// <summary>
    /// Adds a new bot button to the lateral bar.
    /// </summary>
    public void AddBotButton()
    {
        BotButtonList.Add(new() { Content = "Bot" });
        this.RaisePropertyChanged(nameof(BotButtonList));
    }

    public void ShowMessage(string message)
    {
        var window = new DialogWindowView(message);
        window.Show();
    }
}