using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Immutable;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using NadekoHub.DesignData.Common;
using NadekoHub.Models.Config;
using NadekoHub.Models.EventArguments;
using NadekoHub.Services.Mocks;
using NadekoHub.ViewModels.Controls;
using SkiaImageView;
using SkiaSharp;

namespace NadekoHub.Views.Controls;

/// <summary>
/// View for the lateral bar with home, bot, and configuration buttons.
/// </summary>
public partial class LateralBarView : ReactiveUserControl<LateralBarViewModel>
{
    private static readonly Cursor _pointingHandCursor = new(StandardCursorType.Hand);
    private static readonly Cursor _arrow = new(StandardCursorType.Arrow);
    private readonly ReadOnlyAppConfig _appConfig;

    /// <summary>
    /// Raised when the user clicks a bot button.
    /// </summary>
    public event EventHandler<Button, RoutedEventArgs>? BotButtonClick;

    /// <summary>
    /// Designer's constructor. Use the parameterized constructor instead.
    /// </summary>
    [Obsolete(WindowConstants.DesignerCtorWarning, true)]
    public LateralBarView() : this(DesignStatics.Services.GetRequiredService<MockAppConfigManager>().AppConfig)
    {
    }

    /// <summary>
    /// Creates the lateral bar of the application.
    /// </summary>
    /// <param name="appConfig">The application settings.</param>
    public LateralBarView(ReadOnlyAppConfig appConfig)
    {
        _appConfig = appConfig;
        InitializeComponent();
    }

    /// <summary>
    /// Changes the avatar of a bot button in the lateral bar.
    /// </summary>
    /// <param name="eventArgs">The event arguments emited when a bot avatar is changed.</param>
    public Task UpdateBotButtonAvatarAsync(AvatarChangedEventArgs eventArgs)
    {
        var buttonAvatar = FindAvatarComponent(eventArgs.BotId);
        var oldAvatar = buttonAvatar.Source as SKBitmap;

        buttonAvatar.Source = eventArgs.Avatar;
        oldAvatar?.Dispose();   // Dispose the old avatar

        return Task.CompletedTask;
    }

    /// <summary>
    /// Applies the selection outline to the specified bot button.
    /// </summary>
    /// <param name="button">The bot button.</param>
    /// <exception cref="InvalidOperationException">Occurs when the visual tree has an unexpected structure.</exception>
    public void ApplyBotButtonBorder(Button button)
    {
        if (!Utilities.TryCastTo<Border>(button.Parent?.Parent, out var border))
            throw new InvalidOperationException("Visual tree has an unexpected structure.");

        if (!Utilities.TryCastTo<ImmutableSolidColorBrush>(this.FindResource(base.ActualThemeVariant, "BotSelectionColor"), out var resourceColor))
            return;

        border.BorderBrush = resourceColor;
    }

    /// <summary>
    /// Applies a transparent outline to all bot buttons.
    /// </summary>
    public void ResetBotButtonBorders()
    {
        foreach (var border in ButtonList.Children.Cast<Border>())
            border.BorderBrush = AppStatics.TransparentColorBrush;
    }

    /// <summary>
    /// Routes a button click from the view to <see cref="BotButtonClick"/>.
    /// </summary>
    /// <param name="sender">The bot button that was clicked.</param>
    /// <param name="eventArgs">The event arguments.</param>
    private void LoadBotViewModel(object sender, RoutedEventArgs eventArgs)
        => BotButtonClick?.Invoke((Button)sender, eventArgs);

    /// <summary>
    /// Loads the bot avatar when the buttons on the lateral bar are rendered.
    /// </summary>
    /// <param name="sender">The <see cref="Panel"/> that contains the bot buttons and avatars.</param>
    /// <param name="eventArgs">The event arguments.</param>
    /// <remarks>This is executed each time one of the buttons is rendered.</remarks>
    /// <exception cref="InvalidOperationException">Occurs when the visual tree has an unexpected structure.</exception>
    private void OnBotButtonLoad(object? sender, VisualTreeAttachmentEventArgs eventArgs)
    {
        if (!Utilities.TryCastTo<Panel>(sender, out var panel)
            || !Utilities.TryCastTo<SKImageView>(((Border)panel.Children[0]).Child, out var botAvatar)
            || !Utilities.TryCastTo<Button>(panel.Children[1], out var button)
            || !Utilities.TryCastTo<Guid>(button.Content, out var botId))
            throw new InvalidOperationException("Visual tree has an unexpected structure.");

        // Set the avatar
        (botAvatar.Source as IDisposable)?.Dispose();
        botAvatar.Source = Utilities.LoadLocalImage(_appConfig.BotEntries[botId].AvatarUri);

        // Tunnel press and release events directly to the handling methods.
        // This is necessary because the Click event gobbles these up in the axaml file.
        button.AddHandler(PointerPressedEvent, DownsizeBotAvatar, RoutingStrategies.Tunnel);
        button.AddHandler(PointerReleasedEvent, UpsizeBotAvatar, RoutingStrategies.Tunnel);
    }

    /// <summary>
    /// Decreases the size of the bot avatar by 1 pixel.
    /// </summary>
    /// <param name="sender">The <see cref="Button"/> that was pressed.</param>
    /// <param name="eventArgs">The event arguments.</param>
    /// <exception cref="InvalidOperationException">Occurs when <paramref name="sender"/> is not a <see cref="Button"/>.</exception>
    private void DownsizeBotAvatar(object? sender, PointerPressedEventArgs eventArgs)
    {
        if (!Utilities.TryCastTo<Button>(sender, out var button))
            throw new InvalidOperationException($"Sender is not a {nameof(Button)}.");

        var botAvatar = FindAvatarComponent(button);

        botAvatar.Width = botAvatar.DesiredSize.Width - 1;
        botAvatar.Height = botAvatar.DesiredSize.Height - 1;
    }

    /// <summary>
    /// Increases the size of the bot avatar by 1 pixel.
    /// </summary>
    /// <param name="sender">The <see cref="Button"/> that was released.</param>
    /// <param name="eventArgs">The event arguments.</param>
    /// <exception cref="InvalidOperationException">Occurs when <paramref name="sender"/> is not a <see cref="Button"/>.</exception>
    private void UpsizeBotAvatar(object? sender, PointerReleasedEventArgs eventArgs)
    {
        if (!Utilities.TryCastTo<Button>(sender, out var button))
            throw new InvalidOperationException($"Sender is not a {nameof(Button)}.");

        var botAvatar = FindAvatarComponent(button);

        botAvatar.Width = botAvatar.DesiredSize.Width + 1;
        botAvatar.Height = botAvatar.DesiredSize.Height + 1;
    }

    /// <summary>
    /// Finds the view component in the lateral bar associated with the specified <paramref name="component"/>.
    /// </summary>
    /// <typeparam name="T">The type of the component.</typeparam>
    /// <param name="component">The component whose <see cref="ContentControl.Content"/> is the Id of the bot.</param>
    /// <returns>The avatar view of the bot.</returns>
    /// <exception cref="InvalidOperationException">Occurs when the component's content is not a Guid.</exception>
    private SKImageView FindAvatarComponent<T>(T component) where T : ContentControl
    {
        return (!Utilities.TryCastTo<Guid>(component.Content, out var botId))
            ? throw new InvalidOperationException($"{nameof(T)} does not contain a bot Id.")
            : FindAvatarComponent(botId);
    }

    /// <summary>
    /// Finds the view component in the lateral bar associated with the specified <paramref name="botId"/>.
    /// </summary>
    /// <param name="botId">The Id of the bot.</param>
    /// <returns>The avatar view of the bot.</returns>
    /// <exception cref="InvalidOperationException">Occurs when avatar view is not found.</exception>
    private SKImageView FindAvatarComponent(Guid botId)
    {
        return this.ButtonList.Children
            .Cast<Border>()
            .Select(x => x.Child)
            .Cast<Panel>()
            .Where(x => botId.Equals((x.Children[1] as Button)?.Content))
            .SelectMany(x => x.Children)
            .OfType<Border>()
            .Select(x => x.Child)
            .Cast<SKImageView>()
            .First();
    }
}