using Microsoft.Extensions.DependencyInjection;
using NadekoUpdater.DesignData.Common;
using NadekoUpdater.ViewModels.Controls;
using NadekoUpdater.ViewModels.Windows;

namespace NadekoUpdater.DesignData.Windows;

/// <summary>
/// Mock view-model for <see cref="AppViewModel"/>.
/// </summary>
public sealed class DesignAppViewModel : AppViewModel
{
    /// <summary>
    /// Creates a mock <see cref="AppViewModel"/> to be used at design-time.
    /// </summary>
    public DesignAppViewModel() : base(
            DesignStatics.Services.GetRequiredService<LateralBarViewModel>(),
            DesignStatics.Services.GetRequiredService<HomeViewModel>()
        )
    {
    }
}
