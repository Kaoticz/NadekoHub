using Avalonia.Controls;
using Avalonia.Controls.Templates;
using NadekoUpdater.ViewModels;
using System;

namespace NadekoUpdater;

public class ViewLocator : IDataTemplate
{
    public bool Match(object? data)
        => data is ViewModelBase;

    public Control Build(object? data)
    {
        var name = data?.GetType().FullName?.Replace("ViewModel", "View");
        var type = (string.IsNullOrWhiteSpace(name)) ? null : Type.GetType(name);

        return (type is not null)
            ? (Control)Activator.CreateInstance(type)!
            : new TextBlock { Text = "Not Found: " + name };
    }
}