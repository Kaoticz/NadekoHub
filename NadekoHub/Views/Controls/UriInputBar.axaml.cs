using Avalonia.ReactiveUI;
using NadekoHub.ViewModels.Controls;

namespace NadekoHub.Views.Controls;

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