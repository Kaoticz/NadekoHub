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
            this.LoadBotButtons(botEntryManager.AppConfig.BotEntries);

            // Or use WhenActivated to execute logic
            // when the view model gets deactivated.
            Disposable.Create(() => BotButtonList.Clear())
                .DisposeWith(disposables);
        });
    }

    /// <summary>
    /// Adds a new bot button to the lateral bar.
    /// </summary>
    public async Task AddBotButtonAsync()
    {
        var (_, botEntry) = await _botEntryManager.CreateBotEntryAsync();

        BotButtonList.Add(new() { Content = botEntry.Name });
        this.RaisePropertyChanged(nameof(BotButtonList));
    }

    /// <summary>
    /// Loads the bot buttons to the lateral bar.
    /// </summary>
    /// <param name="botEntires">The bot entries.</param>
    private void LoadBotButtons(IReadOnlyDictionary<uint, BotInstanceInfo> botEntires)
    {
        var botInfos = botEntires
            .OrderBy(x => x.Key)
            .Select(x => x.Value);

        foreach (var botEntry in botInfos)
            BotButtonList.Add(new() { Content = botEntry.Name });

        this.RaisePropertyChanged(nameof(BotButtonList));
    }
}