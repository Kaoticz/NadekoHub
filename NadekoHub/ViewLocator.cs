using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using NadekoHub.ViewModels.Abstractions;

namespace NadekoHub;

/// <summary>
/// An object that creates controls out of view-models.
/// </summary>
public sealed class ViewLocator : IDataTemplate
{
    /// <inheritdoc />
    public bool Match(object? data)
        => data is ViewModelBase;

    /// <inheritdoc />
    /// <remarks>Receives a view-model and returns its corresponding view.</remarks>
    public Control Build(object? data)
    {
        return (data is ViewModelBase viewModel && viewModel.GetType().BaseType?.GenericTypeArguments[0] is Type controlType)
            ? (Application.Current as App)?.Services.GetService(controlType) as Control
                ?? new TextBlock { Text = $"View-model of type \"{data?.GetType().FullName ?? "null"}\" is not registered in the IoC container.", TextWrapping = TextWrapping.WrapWithOverflow }
            : new TextBlock { Text = $"Component of type \"{data?.GetType().FullName ?? "null"}\" is not a valid view-model.", TextWrapping = TextWrapping.WrapWithOverflow };
    }
}