using NadekoHub.ViewModels.Controls;

namespace NadekoHub.DesignData.Controls;

/// <summary>
/// Mock view-model for <see cref="FakeConsoleViewModel"/>.
/// </summary>
public sealed class DesignFakeConsoleViewModel : FakeConsoleViewModel
{
    /// <summary>
    /// Creates a mock <see cref="FakeConsoleViewModel"/> to be used at design-time.
    /// </summary>
    public DesignFakeConsoleViewModel() : base()
        => Watermark = "Sample watermark.";
}
