using System;
using System.Diagnostics;
using Avalonia.Controls;
using NadekoUpdater.ViewModels.Abstractions;
using NadekoUpdater.ViewModels.Controls;
using NadekoUpdater.Views.Controls;

namespace NadekoUpdater.ViewModels.Windows;

/// <summary>
/// View-model for the main window.
/// </summary>
public sealed class MainWindowViewModel : WindowViewModelBase
{
    /// <summary>
    /// View-model instance of a <see cref="LateralBarView"/>.
    /// </summary>
    public LateralBarViewModel LateralBarInstance { get; }

    /// <summary>
    /// Creates a view-model for the main window.
    /// </summary>
    /// <param name="window">The view of the main window.</param>
    /// <param name="parentWindow">The view that owns this view-model or <see langword="null"/> if there isn't one.</param>
    /// <exception cref="InvalidOperationException">Occurs if the view-model of <see cref="LateralBarView"/> is not a <see cref="LateralBarViewModel"/>.</exception>
    public MainWindowViewModel(Window window, Window? parentWindow = null) : base(window, parentWindow)
    {
        var lateralBarView = new LateralBarView(window);
        LateralBarInstance = lateralBarView.DataContext as LateralBarViewModel
            ?? throw new InvalidOperationException(
                $"The view-model of a {nameof(LateralBarView)} is expected to be a {nameof(LateralBarViewModel)} object, " +
                $"but was {lateralBarView.DataContext?.GetType().FullName ?? "null"} instead."
            );
    }

    /// <summary>
    /// Opens the specified URL in the system's default browser.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    public void OpenUrl(string url)
        => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
}