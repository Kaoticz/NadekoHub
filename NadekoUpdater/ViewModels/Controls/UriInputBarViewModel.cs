using NadekoUpdater.ViewModels.Abstractions;
using NadekoUpdater.Views.Controls;
using System.Diagnostics;
using Avalonia.Platform.Storage;
using ReactiveUI;
using Kotz.Events;
using NadekoUpdater.Models.EventArguments;

namespace NadekoUpdater.ViewModels.Controls;

/// <summary>
/// Represents a text box for inputting the absolute path of a directory.
/// </summary>
public class UriInputBarViewModel : ViewModelBase<UriInputBar>
{
    private static readonly FolderPickerOpenOptions _folderPickerOptions = new();
    private string _lastValidUri = AppStatics.AppDefaultConfigDirectoryUri;
    private bool _isDirectoryValid = false;
    private string _currentUri = string.Empty;
    private readonly IStorageProvider _storageProvider;

    /// <summary>
    /// Triggers when a valid uri path is set.
    /// </summary>
    public event EventHandler<UriInputBarViewModel, UriInputBarEventArgs>? OnValidUri;

    /// <summary>
    /// Determines whether the path in <see cref="CurrentUri"/> is valid or not.
    /// </summary>
    public bool IsValidUri
    {
        get => _isDirectoryValid;
        private set => this.RaiseAndSetIfChanged(ref _isDirectoryValid, value);
    }

    /// <summary>
    /// The value currently set to the text box.
    /// </summary>
    public string CurrentUri
    {
        get => _currentUri;
        set
        {
            IsValidUri = IsValidDirectory(value);
            this.RaiseAndSetIfChanged(ref _currentUri, value);

            if (IsValidUri)
            {
                OnValidUri?.Invoke(this, new(_lastValidUri, value));
                _lastValidUri = value;
            }
        }
    }

    /// <summary>
    /// Creates a text box for inputting the absolute path of a directory.
    /// </summary>
    /// <param name="storageProvider">The storage provider of the active window.</param>
    public UriInputBarViewModel(IStorageProvider storageProvider)
        => _storageProvider = storageProvider;

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

        CurrentUri = Path.GetFullPath(selectedUri ?? CurrentUri);
    }

    /// <summary>
    /// Checks if the specified uri points to a valid directory.
    /// </summary>
    /// <param name="directoryUri">The absolute path to a directory.</param>
    /// <returns><see langword="true"/> if the directory is valid, <see langword="false"/> otherwise.</returns>
    private bool IsValidDirectory(string directoryUri)
        => Directory.Exists(directoryUri) && HasWritePermission(directoryUri);

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