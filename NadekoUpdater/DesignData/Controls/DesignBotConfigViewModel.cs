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
    public DesignBotConfigViewModel() : base(
            DesignStatics.Services.GetRequiredService<AppConfigManager>(),
            DesignStatics.Services.GetRequiredService<UriInputBarViewModel>()
        )
    {
    }
}