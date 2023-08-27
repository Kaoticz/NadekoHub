using Microsoft.Extensions.DependencyInjection;
using NadekoUpdater.DesignData.Common;
using NadekoUpdater.Services.Abstractions;
using NadekoUpdater.Services.Mocks;
using NadekoUpdater.ViewModels.Controls;
using NadekoUpdater.Views.Windows;

namespace NadekoUpdater.DesignData.Controls;

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