using Avalonia.Controls;
using NadekoUpdater.ViewModels.Abstractions;
using NadekoUpdater.Views.Controls;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace NadekoUpdater.ViewModels.Controls;

/// <summary>
/// View-model for <see cref="LateralBarView"/>, the lateral bar with home, bot, and configuration buttons.
/// </summary>
public class LateralBarViewModel : ViewModelBase<LateralBarView>
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
}