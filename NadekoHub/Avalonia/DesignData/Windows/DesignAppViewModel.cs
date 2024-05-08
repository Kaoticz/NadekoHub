using Microsoft.Extensions.DependencyInjection;
using NadekoHub.Avalonia.DesignData.Common;
using NadekoHub.Features.AppWindow.ViewModels;
using NadekoHub.Features.Home.ViewModels;

namespace NadekoHub.Avalonia.DesignData.Windows;

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