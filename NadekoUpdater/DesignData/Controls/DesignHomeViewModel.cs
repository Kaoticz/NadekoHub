using NadekoUpdater.ViewModels.Controls;

namespace NadekoUpdater.DesignData.Controls;

/// <summary>
/// Mock view-model for <see cref="HomeViewModel"/>.
/// </summary>
public sealed class DesignHomeViewModel : HomeViewModel
{
    /// <summary>
    /// Creates a mock <see cref="HomeViewModel"/> to be used at design-time.
    /// </summary>
    public DesignHomeViewModel() : base()
    {
    }
}
