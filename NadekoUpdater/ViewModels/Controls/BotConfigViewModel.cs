using Avalonia.Media.Imaging;
using Avalonia.Platform;
using NadekoUpdater.Services;
using NadekoUpdater.ViewModels.Abstractions;
using NadekoUpdater.Views.Controls;
using ReactiveUI;

namespace NadekoUpdater.ViewModels.Controls;

/// <summary>
/// View-model for <see cref="BotConfigView"/>, the window with settings and controls for a specific bot instance.
/// </summary>
public class BotConfigViewModel : ViewModelBase<BotConfigView>
{
    private Bitmap _botAvatar = LoadLocalImage();
    private string _botName = string.Empty;
    private string _directoryHint = string.Empty;
    private readonly AppConfigManager _appConfigManager;

    /// <summary>
    /// The position of this bot instance in the lateral bot list.
    /// </summary>
    public uint Position { get; private set; }

    /// <summary>
    /// The bar that defines where the bot instance should be saved to.
    /// </summary>
    public UriInputBarViewModel BotDirectoryUriBar { get; }

    /// <summary>
    /// The bot avatar to be displayed on the front-end.
    /// </summary>
    public Bitmap BotAvatar
    {
        get => _botAvatar;
        set => this.RaiseAndSetIfChanged(ref _botAvatar, value);
    }

    /// <summary>
    /// The hint for <see cref="BotDirectoryUriBar"/>.
    /// </summary>
    public string DirectoryHint
    {
        get => _directoryHint;
        private set => this.RaiseAndSetIfChanged(ref _directoryHint, value);
    }

    /// <summary>
    /// The name of the bot.
    /// </summary>
    public string BotName
    {
        get => _botName;
        set
        {
            DirectoryHint = $"Select the absolute path to the bot directory. For example: {Path.Combine(_appConfigManager.AppConfig.BotsDirectoryUri, value)}";
            this.RaiseAndSetIfChanged(ref _botName, value);
        }
    }

    /// <summary>
    /// Creates a view-model for <see cref="BotConfigView"/>.
    /// </summary>
    /// <param name="appConfigManager">The app settings manager.</param>
    /// <param name="botDirectoryUriBar">The text box with the path to the directory where the bot instance is.</param>
    public BotConfigViewModel(AppConfigManager appConfigManager, UriInputBarViewModel botDirectoryUriBar)
    {
        _appConfigManager = appConfigManager;
        BotDirectoryUriBar = botDirectoryUriBar;
    }

    /// <summary>
    /// Sets up the appropriate state of this view-model after initialization.
    /// </summary>
    /// <param name="botEntryPosition">The position of the bot in the lateral bot list.</param>
    /// <returns>This view-model.</returns>
    public BotConfigViewModel FinishInitialization(uint botEntryPosition)
    {
        var botEntry = _appConfigManager.AppConfig.BotEntries[botEntryPosition];

        BotDirectoryUriBar.CurrentUri = botEntry.InstanceDirectoryUri;
        BotAvatar = LoadLocalImage(botEntry.AvatarUri);
        BotName = botEntry.Name;
        Position = botEntryPosition;

        return this;
    }

    /// <summary>
    /// Loads the image at the specified location or the bot avatar placeholder one if it was not found.
    /// </summary>
    /// <param name="uri">The absolute path to the image file or <see langword="null"/> to get the avatar placeholder.</param>
    /// <returns>The requested image or the default bot avatar placeholder.</returns>
    private static Bitmap LoadLocalImage(string? uri = default)
    {
        return (File.Exists(uri))
            ? new(AssetLoader.Open(new Uri(uri)))
            : new(AssetLoader.Open(new Uri(AppStatics.BotAvatarPlaceholderUri)));
    }
}