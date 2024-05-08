using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using NadekoHub.Avalonia.DesignData.Common;
using NadekoHub.Features.Shared.ViewModels;

namespace NadekoHub.Avalonia.DesignData.Controls;

/// <summary>
/// Mock view-model for <see cref="UriInputBarViewModel"/>.
/// </summary>
public sealed class DesignUriInputBarViewModel : UriInputBarViewModel
{
    /// <summary>
    /// Creates a mock <see cref="UriInputBarViewModel"/> to be used at design-time.
    /// </summary>
    public DesignUriInputBarViewModel() : base(DesignStatics.Services.GetRequiredService<IStorageProvider>())
    {
    }
}