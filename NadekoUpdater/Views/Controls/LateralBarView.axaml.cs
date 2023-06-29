using Avalonia.Controls;
using NadekoUpdater.ViewModels.Controls;

namespace NadekoUpdater.Views.Controls;

/// <summary>
/// View for the lateral bar with home, bot, and configuration buttons.
/// </summary>
public partial class LateralBarView : UserControl
{
    /// <summary>
    /// Creates a view for the lateral bar with home, bot, and configuration buttons.
    /// </summary>
    /// <param name="window">The view this control is being rendered into.</param>
    public LateralBarView(Window window)
    {
        InitializeComponent();
        base.DataContext = new LateralBarViewModel(this, window);
    }
}