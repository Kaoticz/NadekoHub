using Avalonia.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace NadekoUpdater.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public LateralBarViewModel LateralBarInstance { get; } = new();

    public ObservableCollection<Button> BotButtonList { get; } = new();
    
    public void OpenUrl(string url)
        => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
}