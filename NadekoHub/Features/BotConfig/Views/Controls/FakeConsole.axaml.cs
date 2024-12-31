using Avalonia.ReactiveUI;
using NadekoHub.Features.BotConfig.ViewModels;

namespace NadekoHub.Features.BotConfig.Views.Controls;

/// <summary>
/// Represents a control that mimics the appearance of a terminal emulator.
/// </summary>
public partial class FakeConsole : ReactiveUserControl<FakeConsoleViewModel>
{
    /// <summary>
    /// Creates a control that mimics the appearance of a terminal emulator.
    /// </summary>
    public FakeConsole()
        => InitializeComponent();
}