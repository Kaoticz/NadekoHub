using Microsoft.Extensions.DependencyInjection;
using NadekoHub.Avalonia.DesignData.Common;
using NadekoHub.Features.AppConfig.Services.Mocks;
using NadekoHub.Features.AppWindow.Views.Windows;
using NadekoHub.Features.BotConfig.Services.Abstractions;
using NadekoHub.Features.BotConfig.Services.Mocks;
using NadekoHub.Features.BotConfig.ViewModels;
using NadekoHub.Features.Common.ViewModels;

namespace NadekoHub.Avalonia.DesignData.Controls;

/// <summary>
/// Mock view-model for <see cref="BotConfigViewModel"/>.
/// </summary>
public sealed class DesignBotConfigViewModel : BotConfigViewModel
{
    /// <summary>
    /// Creates a mock <see cref="BotConfigViewModel"/> to be used at design-time.
    /// </summary>
    public DesignBotConfigViewModel() : base(
            DesignStatics.Services.GetRequiredService<MockAppConfigManager>(),
            DesignStatics.Services.GetRequiredService<AppView>(),
            DesignStatics.Services.GetRequiredService<UriInputBarViewModel>(),
            DesignStatics.Services.GetRequiredService<DependencyButtonViewModel>(),
            DesignStatics.Services.GetRequiredService<FakeConsoleViewModel>(),
            DesignStatics.Services.GetRequiredService<MockNadekoResolver>(),
            DesignStatics.Services.GetRequiredService<IBotOrchestrator>(),
            DesignStatics.Services.GetRequiredService<ILogWriter>()
        )
    {
    }
}