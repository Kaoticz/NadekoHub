using Avalonia.Controls;
using NadekoUpdater.ViewModels;
using ReactiveUI;
using System.Threading.Tasks;

namespace NadekoUpdater.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}