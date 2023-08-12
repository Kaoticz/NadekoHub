using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Kotz.Events;
using NadekoUpdater.ViewModels.Controls;

namespace NadekoUpdater.Views.Controls;

/// <summary>
/// View for the lateral bar with home, bot, and configuration buttons.
/// </summary>
public partial class LateralBarView : ReactiveUserControl<LateralBarViewModel>
{
    /// <summary>
    /// Raised when the user clicks a bot button.
    /// </summary>
    public event EventHandler<Button, RoutedEventArgs>? BotButtonClick;

    /// <summary>
    /// Creates the lateral bar of the application.
    /// </summary>
    public LateralBarView()
        => InitializeComponent();

    /// <summary>
    /// Routes a button click from the view to <see cref="BotButtonClick"/>.
    /// </summary>
    /// <param name="sender">The bot button that was clicked.</param>
    /// <param name="eventArgs">The event arguments.</param>
    private void LoadBotViewModel(object sender, RoutedEventArgs eventArgs)
        => BotButtonClick?.Invoke((Button)sender, eventArgs);
}