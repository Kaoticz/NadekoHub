using Avalonia.Controls;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace NadekoUpdater.ViewModels;

public class LateralBarViewModel : ViewModelBase
{
    public ObservableCollection<Button> BotButtonList { get; } = new();

    public void AddBotButton()
    {
        BotButtonList.Add(new() { Content = "Bot" });
        this.RaisePropertyChanged(nameof(BotButtonList));
    }
}
