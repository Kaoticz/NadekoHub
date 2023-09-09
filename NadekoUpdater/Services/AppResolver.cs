using NadekoUpdater.Services.Abstractions;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NadekoUpdater.Services;

/// <summary>
/// Defines a service that updates this application.
/// </summary>
public sealed class AppResolver : IAppResolver
{
    private static readonly string _tempDirectory = Path.GetTempPath();
    private static readonly string _downloadedFileName = GetDownloadFileName();
    private static readonly string? _currentUpdaterVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
    private readonly IHttpClientFactory _httpClientFactory;

    /// <inheritdoc/>
    public string DependencyName { get; } = "NadekoUpdater";

    /// <inheritdoc/>
    public string OldFileSuffix { get; } = "_old";

    /// <inheritdoc/>
    public string FileName { get; }
   
    /// <inheritdoc/>
    public string BinaryUri { get; }

    /// <summary>
    /// Creates a service that updates this application.
    /// </summary>
    /// <param name="httpClientFactory">The Http client factory.</param>
    public AppResolver(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        FileName = (OperatingSystem.IsWindows()) ? "NadekoUpdater.exe" : "NadekoUpdater";
        BinaryUri = Path.Combine(AppContext.BaseDirectory, FileName);
    }

    /// <inheritdoc/>
    public ValueTask<string?> GetCurrentVersionAsync(CancellationToken cToken = default)
        => ValueTask.FromResult(_currentUpdaterVersion);

    /// <inheritdoc/>
    public void LaunchNewVersion()
        => Utilities.StartProcess(BinaryUri);

    /// <returns>
    /// <see langword="true"/> if the updater can be updated,
    /// <see langword="false"/> if the updater is up-to-date,
    /// <see langword="null"/> if the updater cannot update itself.
    /// </returns>
    /// <inheritdoc/>
    public async ValueTask<bool?> CanUpdateAsync(CancellationToken cToken = default)
    {
        if (!Utilities.CanWriteTo(AppContext.BaseDirectory))
            return null;

        var currentVersion = await GetCurrentVersionAsync(cToken);

        if (currentVersion is null)
            return null;

        var latestVersion = await GetLatestVersionAsync(cToken);

        if (latestVersion.Equals(currentVersion, StringComparison.Ordinal))
            return false;

        var http = _httpClientFactory.CreateClient();

        return await http.IsUrlValidAsync($"https://github.com/Kaoticz/NadekoBot/releases/download/{latestVersion}/{_downloadedFileName}", cToken);
    }

    /// <inheritdoc/>
    public bool RemoveOldFiles()
    {
        var result = false;

        foreach (var file in Directory.GetFiles(AppContext.BaseDirectory).Where(x => x.EndsWith(OldFileSuffix)))
            result |= Utilities.TryDeleteFile(file);

        return result;
    }

    /// <inheritdoc/>
    public async ValueTask<string> GetLatestVersionAsync(CancellationToken cToken = default)
    {
        // TODO: pull from Gitlab
        var http = _httpClientFactory.CreateClient(AppConstants.NoRedirectClient);

        var response = await http.GetAsync("https://github.com/Kaoticz/NadekoBot/releases/latest", cToken);

        var lastSlashIndex = response.Headers.Location?.OriginalString.LastIndexOf('/')
            ?? throw new InvalidOperationException("Failed to get the latest NadekoBotUpdater version.");

        return response.Headers.Location.OriginalString[(lastSlashIndex + 1)..];
    }

    /// <inheritdoc/>
    public async ValueTask<(string? OldVersion, string? NewVersion)> InstallOrUpdateAsync(string installationUri, CancellationToken cToken = default)
    {
        var currentVersion = await GetCurrentVersionAsync(cToken);
        var latestVersion = await GetLatestVersionAsync(cToken);

        if (latestVersion.Equals(currentVersion, StringComparison.Ordinal))
            return (currentVersion, null);

        var http = _httpClientFactory.CreateClient();
        var appTempLocation = Path.Combine(_tempDirectory, _downloadedFileName[..(_downloadedFileName.LastIndexOf('.'))]);
        var zipTempLocation = Path.Combine(_tempDirectory, _downloadedFileName);

        try
        {
            // TODO: pull from Gitlab
            using var downloadStream = await http.GetStreamAsync($"https://github.com/Kaoticz/NadekoBot/releases/download/{latestVersion}/{_downloadedFileName}", cToken);

            // Save the zip file
            using (var fileStream = new FileStream(zipTempLocation, FileMode.Create))
                await downloadStream.CopyToAsync(fileStream, cToken);

            // Extract the zip file
            await Task.Run(() => ZipFile.ExtractToDirectory(zipTempLocation, _tempDirectory), cToken);

            // Move the new binary and its dependencies
            var newFilesUris = Directory.EnumerateFiles(appTempLocation);

            foreach (var newFileUri in newFilesUris)
            {
                var destinationUri = Path.Combine(AppContext.BaseDirectory, newFileUri[(newFileUri.LastIndexOf(Path.DirectorySeparatorChar) + 1)..]);
                
                // Rename the original file from "file" to "file_old".
                if (File.Exists(destinationUri))
                    File.Move(destinationUri, destinationUri + OldFileSuffix);

                // Move the new file to the application's directory.
                if (Environment.OSVersion.Platform is not PlatformID.Unix)
                    File.Move(newFileUri, destinationUri, true);
                else
                {
                    // Circumvent this issue on Unix systems: https://github.com/dotnet/runtime/issues/31149
                    using var moveProcess = Utilities.StartProcess("mv", $"\"{newFileUri}\" \"{destinationUri}\"");
                    await moveProcess.WaitForExitAsync(cToken);
                }
            }

            // Mark the new binary file as executable.
            if (Environment.OSVersion.Platform is PlatformID.Unix)
            {
                using var chmod = Utilities.StartProcess("chmod", $"+x \"{BinaryUri}\"");
                await chmod.WaitForExitAsync(cToken);
            }

            return (currentVersion, latestVersion);
        }
        finally
        {
            // Cleanup
            Utilities.TryDeleteFile(zipTempLocation);
            Utilities.TryDeleteDirectory(appTempLocation);
        }
    }

    /// <summary>
    /// Gets the name of the file to be downloaded.
    /// </summary>
    /// <returns>The name of the file to be downloaded.</returns>
    /// <exception cref="NotSupportedException">Occurs when this method is used in an unsupported system.</exception>
    private static string GetDownloadFileName()
    {
        return RuntimeInformation.OSArchitecture switch
        {
            // Windows
            Architecture.X64 when OperatingSystem.IsWindows() => "NadekoUpdater_win-x64.zip",
            Architecture.Arm64 when OperatingSystem.IsWindows() => "NadekoUpdater_win-arm64.zip",

            // Linux
            Architecture.X64 when OperatingSystem.IsLinux() => "NadekoUpdater_linux-x64.zip",
            Architecture.Arm64 when OperatingSystem.IsLinux() => "NadekoUpdater_linux-arm64.zip",

            // MacOS
            Architecture.X64 when OperatingSystem.IsMacOS() => "NadekoUpdater_osx-x64.zip",
            Architecture.Arm64 when OperatingSystem.IsMacOS() => "NadekoUpdater_osx-arm64.zip",
            _ => throw new NotSupportedException($"Architecture of type {RuntimeInformation.OSArchitecture} is not supported by NadekoUpdater on this OS.")
        };
    }
}