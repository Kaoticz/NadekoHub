using Microsoft.Extensions.DependencyInjection;
using NadekoHub.DesignData.Common;
using NadekoHub.Services.Abstractions;
using NadekoHub.Services.Mocks;
using NadekoHub.ViewModels.Controls;
using NadekoHub.Views.Windows;

namespace NadekoHub.DesignData.Controls;

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