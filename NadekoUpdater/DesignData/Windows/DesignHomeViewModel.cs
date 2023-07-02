using Microsoft.Extensions.DependencyInjection;
using NadekoUpdater.DesignData.Common;
using NadekoUpdater.ViewModels.Controls;
using NadekoUpdater.ViewModels.Windows;

namespace NadekoUpdater.DesignData.Windows;

/// <summary>
/// Mock view-model for <see cref="HomeViewModel"/>.
/// </summary>
public sealed class DesignHomeViewModel : HomeViewModel
{
    /// <summary>
    /// Creates a mock <see cref="HomeViewModel"/> to be used at design-time.
    /// </summary>
    public DesignHomeViewModel() : base(DesignStatics.Services.GetRequiredService<LateralBarViewModel>())
    {
    }
}
