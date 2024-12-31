using Microsoft.Extensions.DependencyInjection;
using NadekoHub.Avalonia.DesignData.Common;
using NadekoHub.Features.AppConfig.Services.Abstractions;
using NadekoHub.Features.AppConfig.Services.Mocks;
using NadekoHub.Features.AppConfig.ViewModels;
using NadekoHub.Features.AppWindow.Views.Windows;
using NadekoHub.Features.Common.ViewModels;

namespace NadekoHub.Avalonia.DesignData.Controls;

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