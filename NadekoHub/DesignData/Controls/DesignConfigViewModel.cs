using Microsoft.Extensions.DependencyInjection;
using NadekoHub.DesignData.Common;
using NadekoHub.Services.Abstractions;
using NadekoHub.Services.Mocks;
using NadekoHub.ViewModels.Controls;
using NadekoHub.ViewModels.Windows;
using NadekoHub.Views.Windows;

namespace NadekoHub.DesignData.Controls;

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