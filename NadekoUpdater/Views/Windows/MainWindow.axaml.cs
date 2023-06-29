using Avalonia.Controls;
using NadekoUpdater.ViewModels.Windows;

namespace NadekoUpdater.Views.Windows;

/// <summary>
/// Represents the main window of the application.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Creates the main window of the application.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(this);
    }
}