using NadekoUpdater.ViewModels.Abstractions;
using NadekoUpdater.Views.Controls;
using System.Diagnostics;

namespace NadekoUpdater.ViewModels.Controls;

/// <summary>
/// View-model for the home window, with links to Nadeko resources.
/// </summary>
public class HomeViewModel : ViewModelBase<HomeView>
{
    /// <summary>
    /// Opens the specified URL in the system's default browser.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    public void OpenUrl(string url)
        => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
}
