using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using NadekoUpdater.ViewModels.Controls;
using System.Diagnostics.CodeAnalysis;

namespace NadekoUpdater.Views.Controls;

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
        if (!TryCastTo<Button>(sender, out var button))
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
        if (!TryCastTo<Button>(sender, out var button))
            return;

        button.Opacity = 0.0;
        base.Cursor = _arrow;
    }

    /// <summary>
    /// Safely casts an event sender to a <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to cast to.</typeparam>
    /// <param name="obj">The object to be cast.</param>
    /// <param name="castObject">The cast object.</param>
    /// <returns><see langword="true"/> if the object was successfully cast, <see langword="false"/> otherwise.</returns>
    private bool TryCastTo<T>(object? obj, [MaybeNullWhen(false)] out T castObject) where T : class
    {
        castObject = obj as T;
        return castObject is not default(T);
    }
}