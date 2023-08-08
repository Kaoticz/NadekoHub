using Microsoft.Extensions.DependencyInjection;
using NadekoUpdater.DesignData.Common;
using NadekoUpdater.ViewModels.Controls;

namespace NadekoUpdater.DesignData.Controls;

/// <summary>
/// Mock view-model for <see cref="ConfigViewModel"/>.
/// </summary>
public sealed class DesignConfigViewModel : ConfigViewModel
{
    /// <summary>
    /// Creates a mock <see cref="ConfigViewModel"/> to be used at design-time.
    /// </summary>
    public DesignConfigViewModel() : base(DesignStatics.Services.GetRequiredService<UriInputBarViewModel>())
    {
    }
}