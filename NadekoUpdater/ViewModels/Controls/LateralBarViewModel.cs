using Avalonia.Controls;
using NadekoUpdater.Models;
using NadekoUpdater.Services;
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

    private readonly AppConfigManager _botEntryManager;

    /// <summary>
    /// Creates the view-model for the <see cref="LateralBarView"/>.
    /// </summary>
    /// <param name="botEntryManager">The application's settings manager.</param>
    public LateralBarViewModel(AppConfigManager botEntryManager)
    {
        _botEntryManager = botEntryManager;

        this.WhenActivated(disposables =>
        {
            // Use WhenActivated to execute logic
            // when the view model gets activated.
            this.ReloadBotButtons(botEntryManager.AppConfig.BotEntries);

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

        BotButtonList.Add(new() { Content = botEntry.Position });
        this.RaisePropertyChanged(nameof(BotButtonList));
    }

    /// <summary>
    /// Removes a bot button from the lateral bar.
    /// </summary>
    /// <param name="position">The position of the button to be removed.</param>
    public async ValueTask RemoveBotButtonAsync(uint position)
    {
        await _botEntryManager.DeleteBotEntryAsync(position);
        
        var toRemove = BotButtonList.First(x => x.Content?.Equals(position) is true);
        BotButtonList.Remove(toRemove);

        this.RaisePropertyChanged(nameof(BotButtonList));
    }

    /// <summary>
    /// Loads the bot buttons to the lateral bar.
    /// </summary>
    /// <param name="botEntires">The bot entries.</param>
    private void ReloadBotButtons(IReadOnlyDictionary<uint, BotInstanceInfo> botEntires)
    {
        BotButtonList.Clear();

        var botPositions = botEntires
            .OrderBy(x => x.Key)
            .Select(x => x.Key);

        foreach (var botPosition in botPositions)
            BotButtonList.Add(new() { Content = botPosition });

        this.RaisePropertyChanged(nameof(BotButtonList));
    }
}