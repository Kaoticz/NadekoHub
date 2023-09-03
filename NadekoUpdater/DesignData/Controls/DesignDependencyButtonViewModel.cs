using Microsoft.Extensions.DependencyInjection;
using NadekoUpdater.DesignData.Common;
using NadekoUpdater.ViewModels.Controls;
using NadekoUpdater.Views.Windows;

namespace NadekoUpdater.DesignData.Controls;

/// <summary>
/// Mock view-model for <see cref="DependencyButtonViewModel"/>.
/// </summary>
public sealed class DesignDependencyButtonViewModel : DependencyButtonViewModel
{
    /// <summary>
    /// Creates a mock <see cref="DependencyButtonViewModel"/> to be used at design-time.
    /// </summary>
    public DesignDependencyButtonViewModel() : base(DesignStatics.Services.GetRequiredService<AppView>())
    {
    }
}
