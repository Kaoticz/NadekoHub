using Avalonia.ReactiveUI;
using NadekoUpdater.ViewModels.Controls;

namespace NadekoUpdater.Views.Controls;

/// <summary>
/// The view for the application's settings.
/// </summary>
public partial class ConfigView : ReactiveUserControl<ConfigViewModel>
{
    /// <summary>
    /// Created a view for the application's settings.
    /// </summary>
    public ConfigView()
        => InitializeComponent();
}