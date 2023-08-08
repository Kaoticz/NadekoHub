using NadekoUpdater.ViewModels.Abstractions;
using NadekoUpdater.Views.Controls;
using System.Diagnostics;
using Avalonia.Platform.Storage;
using NadekoUpdater.Views.Windows;
using ReactiveUI;

namespace NadekoUpdater.ViewModels.Controls;

/// <summary>
/// Represents a text box for inputting the absolute path of a directory.
/// </summary>
public class UriInputBarViewModel : ViewModelBase<UriInputBar>
{
    private static readonly FolderPickerOpenOptions _folderPickerOptions = new();
    private string _currentUri = AppStatics.DefaultUserConfigUri;   // TODO: Set to whatever the user prefers
    private bool _isFolderValid = true; // TODO: above
    private readonly IStorageProvider _storageProvider;

    /// <summary>
    /// Determines whether the path in <see cref="CurrentUri"/> is valid or not.
    /// </summary>
    public bool IsDirectoryValid
    {
        get => _isFolderValid;
        set => this.RaiseAndSetIfChanged(ref _isFolderValid, value);
    }

    /// <summary>
    /// The value currently set to the text box.
    /// </summary>
    public string CurrentUri
    {
        get => _currentUri;
        set
        {
            IsDirectoryValid = Directory.Exists(value) && HasWritePermission(value);
            this.RaiseAndSetIfChanged(ref _currentUri, value);
        }
    }

    /// <summary>
    /// Creates a text box for inputting the absolute path of a directory.
    /// </summary>
    /// <param name="mainWindow">The application's main window.</param>
    public UriInputBarViewModel(AppView mainWindow)
        => _storageProvider = mainWindow.StorageProvider;

    /// <summary>
    /// Opens the directory at <paramref name="directoryUri"/>.
    /// </summary>
    /// <param name="directoryUri">Absolute path to the directory to be opened.</param>
    public void OpenFolder(string directoryUri)
        => Process.Start(new ProcessStartInfo(directoryUri) { UseShellExecute = true });

    /// <summary>
    /// Opens a directory picker and sets <see cref="CurrentUri"/> to the selected directory.
    /// </summary>
    public async Task SelectFolderAsync()
    {
        var selectedUri = (await _storageProvider.OpenFolderPickerAsync(_folderPickerOptions))
            .Select(x => x.Path.AbsolutePath)
            .FirstOrDefault();

        CurrentUri = selectedUri ?? CurrentUri;
    }

    /// <summary>
    /// Checks if the application can write to <paramref name="directoryUri"/>.
    /// </summary>
    /// <param name="directoryUri">The absolute path to a directory.</param>
    /// <returns><see langword="true"/> if there is write permission, <see langword="false"/> otherwise.</returns>
    /// <exception cref="PathTooLongException" />
    /// <exception cref="DirectoryNotFoundException" />
    private static bool HasWritePermission(string directoryUri)
    {
        var tempFileUri = Path.Combine(directoryUri, $"{Guid.NewGuid()}.tmp");

        try
        {
            using var fileStream = File.Create(tempFileUri);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        finally
        {
            if (File.Exists(tempFileUri))
                File.Delete(tempFileUri);
        }
    }
}
