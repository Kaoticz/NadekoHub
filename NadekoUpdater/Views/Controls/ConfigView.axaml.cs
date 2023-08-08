using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using NadekoUpdater.DesignData.Common;
using NadekoUpdater.ViewModels.Controls;
using NadekoUpdater.ViewModels.Windows;
using ReactiveUI;

namespace NadekoUpdater.Views.Controls;

/// <summary>
/// The view for the application's settings.
/// </summary>
public partial class ConfigView : ReactiveUserControl<ConfigViewModel>
{
    /// <summary>
    /// Designer's constructor. Use the parameterized constructor instead.
    /// </summary>
    [Obsolete(WindowConstants.DesignerCtorWarning, true)]
    public ConfigView() : this(DesignStatics.Services.GetRequiredService<ConfigViewModel>())
    {
    }

    /// <summary>
    /// Creates a view for the application's settings.
    /// </summary>
    public ConfigView(ConfigViewModel viewModel)
    {
        this.WhenActivated(_ => base.ViewModel = viewModel);
        InitializeComponent();
    }
}