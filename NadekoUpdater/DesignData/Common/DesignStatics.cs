using Avalonia;
using Avalonia.Controls;
using System;

namespace NadekoUpdater.DesignData.Common;

/// <summary>
/// Defines objects useful at design-time.
/// </summary>
internal static class DesignStatics
{
    /// <summary>
    /// Provides the services necessary for design-time rendering of views.
    /// </summary>
    /// <remarks>This property is <see langword="null"/> when the application is not in design mode.</remarks>
    internal static IServiceProvider Services { get; } = (Design.IsDesignMode)
        ? (Application.Current as App)!.Services
        : null!;
}
