using Microsoft.Extensions.DependencyInjection;
using NadekoUpdater.DesignData.Common;
using NadekoUpdater.Services;
using NadekoUpdater.ViewModels.Controls;

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
            DesignStatics.Services.GetRequiredService<AppConfigManager>(),
            DesignStatics.Services.GetRequiredService<UriInputBarViewModel>(),
            DesignStatics.Services.GetRequiredService<DependencyButtonViewModel>()
        )
    {
    }
}