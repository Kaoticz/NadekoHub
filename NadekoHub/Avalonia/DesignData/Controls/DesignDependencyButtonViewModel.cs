using Microsoft.Extensions.DependencyInjection;
using NadekoHub.Avalonia.DesignData.Common;
using NadekoHub.Features.AppWindow.Views.Windows;
using NadekoHub.Features.Shared.ViewModels;

namespace NadekoHub.Avalonia.DesignData.Controls;

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