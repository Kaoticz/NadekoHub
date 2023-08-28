using Avalonia.Controls;
using NadekoUpdater.Models;
using NadekoUpdater.Services.Abstractions;
using NadekoUpdater.ViewModels.Abstractions;
using NadekoUpdater.Views.Controls;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;

namespace NadekoUpdater.ViewModels.Controls;

/// <summary>
/// View-model for <see cref="LateralBarView"/>, the lateral bar with home, bot, and configuration buttons.
/// </summary>
public class LateralBarViewModel : ViewModelBase<LateralBarView>
{
    /// <summary>
    /// Collection of buttons for bot instances.
    /// </summary>
    public ObservableCollection<Button> BotButtonList { get; } = new();

    private readonly IAppConfigManager _botEntryManager;

    /// <summary>
    /// Creates the view-model for the <see cref="LateralBarView"/>.
    /// </summary>
    /// <param name="botEntryManager">The application's settings manager.</param>
    public LateralBarViewModel(IAppConfigManager botEntryManager)
    {
        _botEntryManager = botEntryManager;

        this.WhenActivated(disposables =>
        {
            // Use WhenActivated to execute logic
            // when the view model gets activated.
            ReloadBotButtons(botEntryManager.AppConfig.BotEntries);

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
        var botEntry = await _botEntryManager.CreateBotEntryAsync();

        BotButtonList.Add(new() { Content = botEntry.Id });
        this.RaisePropertyChanged(nameof(BotButtonList));
    }

    /// <summary>
    /// Removes a bot button from the lateral bar.
    /// </summary>
    /// <param name="botId">The Id of the bot.</param>
    public async ValueTask RemoveBotButtonAsync(Guid botId)
    {
        var botEntry = await _botEntryManager.DeleteBotEntryAsync(botId);
        
        var toRemove = BotButtonList.First(x => x.Content?.Equals(botEntry?.Id) is true);
        BotButtonList.Remove(toRemove);

        this.RaisePropertyChanged(nameof(BotButtonList));
    }

    /// <summary>
    /// Loads the bot buttons to the lateral bar.
    /// </summary>
    /// <param name="botEntires">The bot entries.</param>
    private void ReloadBotButtons(IReadOnlyDictionary<Guid, BotInstanceInfo> botEntires)
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