using Avalonia.Controls;
using NadekoHub.Features.Abstractions;
using NadekoHub.Features.AppConfig.Services.Abstractions;
using NadekoHub.Features.AppWindow.Models;
using NadekoHub.Features.AppWindow.Views.Controls;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;

namespace NadekoHub.Features.AppWindow.ViewModels;

/// <summary>
/// View-model for <see cref="LateralBarView"/>, the lateral bar with home, bot, and configuration buttons.
/// </summary>
public class LateralBarViewModel : ViewModelBase<LateralBarView>
{
    private bool _isLateralBarEnabled = true;
    private readonly IAppConfigManager _appConfigManager;

    /// <summary>
    /// Collection of buttons for bot instances.
    /// </summary>
    public ObservableCollection<Button> BotButtonList { get; } = [];

    /// <summary>
    /// Determines whether the buttons on the lateral bar are enabled or not.
    /// </summary>
    public bool IsLateralBarEnabled
    {
        get => _isLateralBarEnabled;
        private set => this.RaiseAndSetIfChanged(ref _isLateralBarEnabled, value);
    }

    /// <summary>
    /// Creates the view-model for the <see cref="LateralBarView"/>.
    /// </summary>
    /// <param name="appConfigManager">The application's settings manager.</param>
    public LateralBarViewModel(IAppConfigManager appConfigManager)
    {
        _appConfigManager = appConfigManager;

        this.WhenActivated(disposables =>
        {
            // Use WhenActivated to execute logic
            // when the view model gets activated.
            ReloadBotButtons(appConfigManager.AppConfig.BotEntries);

            // Or use WhenActivated to execute logic
            // when the view model gets deactivated.
            Disposable.Create(() => BotButtonList.Clear())
                .DisposeWith(disposables);
        });
    }

    /// <summary>
    /// Adds a new bot button to the lateral bar.
    /// </summary>
    public async ValueTask AddBotButtonAsync()
    {
        var botEntry = await _appConfigManager.CreateBotEntryAsync();

        BotButtonList.Add(new() { Content = botEntry.Id });
        this.RaisePropertyChanged(nameof(BotButtonList));
    }

    /// <summary>
    /// Removes a bot button from the lateral bar.
    /// </summary>
    /// <param name="botId">The Id of the bot.</param>
    public async ValueTask RemoveBotButtonAsync(Guid botId)
    {
        var botEntry = await _appConfigManager.DeleteBotEntryAsync(botId);

        var toRemove = BotButtonList.First(x => x.Content?.Equals(botEntry?.Id) is true);
        BotButtonList.Remove(toRemove);

        this.RaisePropertyChanged(nameof(BotButtonList));
    }

    /// <summary>
    /// Enables or disables the buttons on the lateral bar.
    /// </summary>
    /// <param name="enable"><see langword="true"/> to enable the buttons, <see langword="false"/> otherwise.</param>
    public void ToggleEnable(bool enable)
    {
        IsLateralBarEnabled = enable;

        foreach (var button in BotButtonList)
            button.IsEnabled = enable;
    }

    /// <summary>
    /// Loads the bot buttons to the lateral bar.
    /// </summary>
    /// <param name="botEntires">The bot entries.</param>
    public void ReloadBotButtons(IReadOnlyDictionary<Guid, BotInstanceInfo> botEntires)
    {
        BotButtonList.Clear();

        var botIds = botEntires
            .OrderBy(x => x.Value.Position)
            .Select(x => x.Key);

        foreach (var botId in botIds)
            BotButtonList.Add(new() { Content = botId });

        this.RaisePropertyChanged(nameof(BotButtonList));
    }
}