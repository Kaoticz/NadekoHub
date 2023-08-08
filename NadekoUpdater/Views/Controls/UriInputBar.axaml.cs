using Avalonia.ReactiveUI;
using NadekoUpdater.ViewModels.Controls;

namespace NadekoUpdater.Views.Controls;

/// <summary>
/// Represents a text box that receives the absolute path to a directory.
/// </summary>
public partial class UriInputBar : ReactiveUserControl<UriInputBarViewModel>
{
    /// <summary>
    /// Creates a text box that receives the absolute path to a directory.
    /// </summary>
    public UriInputBar()
        => InitializeComponent();
}