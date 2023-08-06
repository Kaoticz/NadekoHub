using Avalonia.ReactiveUI;
using NadekoUpdater.ViewModels.Controls;
using ReactiveUI;
using System.Diagnostics;

namespace NadekoUpdater.Views.Controls;

/// <summary>
/// View for the lateral bar with home, bot, and configuration buttons.
/// </summary>
public partial class LateralBarView : ReactiveUserControl<LateralBarViewModel>
{
    /// <summary>
    /// Creates the lateral bar of the application.
    /// </summary>
    public LateralBarView()
    {
        this.WhenActivated(_ => Debug.WriteLine($"{nameof(LateralBarView)} activated."));
        InitializeComponent();
    }
}