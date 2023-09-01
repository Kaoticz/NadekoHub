using Microsoft.Extensions.DependencyInjection;
using NadekoUpdater.DesignData.Common;
using NadekoUpdater.Services.Abstractions;
using NadekoUpdater.Services.Mocks;
using NadekoUpdater.ViewModels.Controls;
using NadekoUpdater.ViewModels.Windows;
using NadekoUpdater.Views.Windows;

namespace NadekoUpdater.DesignData.Controls;

/// <summary>
/// Mock view-model for <see cref="ConfigViewModel"/>.
/// </summary>
public sealed class DesignConfigViewModel : ConfigViewModel
{
    /// <summary>
    /// Creates a mock <see cref="ConfigViewModel"/> to be used at design-time.
    /// </summary>
    public DesignConfigViewModel() : base(
            DesignStatics.Services.GetRequiredService<MockAppConfigManager>(),
            DesignStatics.Services.GetRequiredService<AppView>(),
            DesignStatics.Services.GetRequiredService<UriInputBarViewModel>(),
            DesignStatics.Services.GetRequiredService<UriInputBarViewModel>(),
            DesignStatics.Services.GetRequiredService<UriInputBarViewModel>(),
            DesignStatics.Services.GetRequiredService<AboutMeViewModel>(),
            DesignStatics.Services.GetRequiredService<IFfmpegResolver>(),
            DesignStatics.Services.GetRequiredService<IYtdlpResolver>()
        )
    {
    }
}