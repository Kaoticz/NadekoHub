using Avalonia.Controls;
using Avalonia.Controls.Templates;
using NadekoUpdater.ViewModels.Abstractions;

namespace NadekoUpdater;

/// <summary>
/// An object that creates controls out of view-models.
/// </summary>
public sealed class ViewLocator : IDataTemplate
{
    /// <inheritdoc />
    public bool Match(object? data)
        => data is ViewModelBase;

    /// <inheritdoc />
    public Control Build(object? data)
    {
        return data switch
        {
            ControlViewModelBase controlViewModel => controlViewModel.Control,
            ViewModelBase viewModel => viewModel.View,
            _ => new TextBlock() { Text = $"Control of type \"{data?.GetType().FullName ?? "null"}\" is not recognized." }
        };
    }
}