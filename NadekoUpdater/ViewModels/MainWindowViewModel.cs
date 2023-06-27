using Avalonia.Controls;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace NadekoUpdater.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<Button> BotButtonList { get; } = new();
    
    public void OpenUrl(string url)
        => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });

    public void AddBotButton()
    {
        BotButtonList.Add(new() { Content = "AAA" });
        this.RaisePropertyChanged(nameof(BotButtonList));
    }
}