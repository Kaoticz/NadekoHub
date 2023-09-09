using Microsoft.Extensions.DependencyInjection;
using NadekoHub.DesignData.Common;
using NadekoHub.ViewModels.Controls;
using NadekoHub.Views.Windows;

namespace NadekoHub.DesignData.Controls;

/// <summary>
/// Mock view-model for <see cref="DependencyButtonViewModel"/>.
/// </summary>
public sealed class DesignDependencyButtonViewModel : DependencyButtonViewModel
{
    /// <summary>
    /// Creates a mock <see cref="DependencyButtonViewModel"/> to be used at design-time.
    /// </summary>
    public DesignDependencyButtonViewModel() : base(DesignStatics.Services.GetRequiredService<AppView>())
    {
    }
}
