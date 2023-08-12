using Avalonia.ReactiveUI;
using NadekoUpdater.ViewModels.Controls;

namespace NadekoUpdater.Views.Controls;

/// <summary>
/// Represents the view with settings and controls for a specific bot instance.
/// </summary>
public partial class BotConfigView : ReactiveUserControl<BotConfigViewModel>
{
    /// <summary>
    /// Creates a view with settings and controls for a specific bot instance.
    /// </summary>
    public BotConfigView()
        => InitializeComponent();
}