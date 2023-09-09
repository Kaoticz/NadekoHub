using Microsoft.Extensions.DependencyInjection;
using NadekoHub.DesignData.Common;
using NadekoHub.Services.Mocks;
using NadekoHub.ViewModels.Controls;

namespace NadekoHub.DesignData.Controls;

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
