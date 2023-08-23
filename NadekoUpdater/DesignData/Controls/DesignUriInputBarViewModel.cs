using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using NadekoUpdater.DesignData.Common;
using NadekoUpdater.ViewModels.Controls;

namespace NadekoUpdater.DesignData.Controls;

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
