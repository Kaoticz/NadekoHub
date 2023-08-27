using Microsoft.Extensions.DependencyInjection;
using NadekoUpdater.DesignData.Common;
using NadekoUpdater.Services.Mocks;
using NadekoUpdater.ViewModels.Controls;

namespace NadekoUpdater.DesignData.Controls;

/// <summary>
/// Mock view-model for <see cref="LateralBarViewModel"/>.
/// </summary>
public sealed class DesignLateralBarViewModel : LateralBarViewModel
{
    /// <summary>
    /// Creates a mock <see cref="LateralBarViewModel"/> to be used at design-time.
    /// </summary>
    public DesignLateralBarViewModel() : base(DesignStatics.Services.GetRequiredService<MockAppConfigManager>())
    {
    }
}
