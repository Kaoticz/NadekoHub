using NadekoHub.Features.Abstractions;
using NadekoHub.Features.AppConfig.Views.Controls;
using ReactiveUI;

namespace NadekoHub.Features.BotConfig.ViewModels;

/// <summary>
/// View-model for <see cref="FakeConsole"/>, the fake console that displays text.
/// </summary>
public class FakeConsoleViewModel : ViewModelBase<FakeConsole>
{
    private string _content = string.Empty;
    private string _watermark = string.Empty;

    /// <summary>
    /// Text to display.
    /// </summary>
    public string Content
    {
        get => _content;
        set => this.RaiseAndSetIfChanged(ref _content, value);
    }

    /// <summary>
    /// Text to display when <see cref="Content"/> is empty.
    /// </summary>
    public string Watermark
    {
        get => _watermark;
        set => this.RaiseAndSetIfChanged(ref _watermark, value);
    }
}