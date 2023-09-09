using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using NadekoHub.ViewModels.Controls;

namespace NadekoHub.Views.Controls;

/// <summary>
/// Represents the view with settings and controls for a specific bot instance.
/// </summary>
public partial class BotConfigView : ReactiveUserControl<BotConfigViewModel>
{
    private static readonly Cursor _pointingHandCursor = new(StandardCursorType.Hand);
    private static readonly Cursor _arrow = new(StandardCursorType.Arrow);

    /// <summary>
    /// Creates a view with settings and controls for a specific bot instance.
    /// </summary>
    public BotConfigView()
        => InitializeComponent();

    /// <summary>
    /// Triggered when the mouse pointer starts hovering the avatar button.
    /// </summary>
    /// <param name="sender">The <see cref="Button"/>.</param>
    /// <param name="eventArgs">The event arguments.</param>
    private void AvatarButtonHover(object? sender, PointerEventArgs eventArgs)
    {
        if (!Utilities.TryCastTo<Button>(sender, out var button))
            return;

        button.Opacity = 3.0;
        base.Cursor = _pointingHandCursor;
    }

    /// <summary>
    /// Triggered when the mouse pointer stops hovering the avatar button.
    /// </summary>
    /// <param name="sender">The <see cref="Button"/>.</param>
    /// <param name="eventArgs">The event arguments.</param>
    private void AvatarButtonUnhover(object? sender, PointerEventArgs eventArgs)
    {
        if (!Utilities.TryCastTo<Button>(sender, out var button))
            return;

        button.Opacity = 0.0;
        base.Cursor = _arrow;
    }
}