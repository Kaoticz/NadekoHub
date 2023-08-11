using Microsoft.Extensions.DependencyInjection;
using NadekoUpdater.DesignData.Common;
using NadekoUpdater.Models.Config;
using NadekoUpdater.ViewModels.Controls;
using NadekoUpdater.Views.Windows;

namespace NadekoUpdater.DesignData.Controls;

/// <summary>
/// Mock view-model for <see cref="UriInputBarViewModel"/>.
/// </summary>
public sealed class DesignUriInputBarViewModel : UriInputBarViewModel
{
    /// <summary>
    /// Creates a mock <see cref="UriInputBarViewModel"/> to be used at design-time.
    /// </summary>
    public DesignUriInputBarViewModel() : base(
            DesignStatics.Services.GetRequiredService<AppView>(),
            DesignStatics.Services.GetRequiredService<ReadOnlyAppConfig>()
        )
    {
    }
}
