using Kotz.Utilities;
using Microsoft.Extensions.Caching.Memory;
using NadekoHub.Features.Home.Models.Api.Github;
using NadekoHub.Features.Home.Services.Abstractions;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace NadekoHub.Features.Home.Services;

/// <summary>
/// Defines a service that updates this application.
/// </summary>
public sealed class AppResolver : IAppResolver
{
    private const string _cachedCurrentVersionKey = "currentVersion:NadekoHub";
    private const string _githubReleasesEndpointUrl = "https://api.github.com/repos/Kaoticz/NadekoHub/releases/latest";
    private const string _githubReleasesRepoUrl = "https://github.com/Kaoticz/NadekoHub/releases/latest";
    private static readonly string _tempDirectory = Path.GetTempPath();
    private static readonly string _downloadedFileName = GetDownloadFileName();
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;

    /// <inheritdoc/>
    public string DependencyName { get; } = "NadekoHub";

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
    /// <param name="memoryCache">The memory cache.</param>
    public AppResolver(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
    {
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        FileName = OperatingSystem.IsWindows() ? "NadekoHub.exe" : "NadekoHub";
        BinaryUri = Path.Join(AppContext.BaseDirectory, FileName);
    }

    /// <inheritdoc/>
    public ValueTask<string?> GetCurrentVersionAsync(CancellationToken cToken = default)
        => ValueTask.FromResult<string?>(AppStatics.AppVersion);

    /// <inheritdoc/>
    public void LaunchNewVersion()
        => KotzUtilities.StartProcess(BinaryUri);

    /// <returns>
    /// <see langword="true"/> if the updater can be updated,
    /// <see langword="false"/> if the updater is up-to-date,
    /// <see langword="null"/> if the updater cannot update itself.
    /// </returns>
    /// <inheritdoc/>
    public async ValueTask<bool?> CanUpdateAsync(CancellationToken cToken = default)
    {
        if (!KotzUtilities.HasWritePermissionAt(AppContext.BaseDirectory))
            return null;

        var currentVersion = await GetCurrentVersionAsync(cToken);

        if (currentVersion is null)
            return null;

        var latestVersion = await GetLatestVersionAsync(cToken);

        if (Version.Parse(latestVersion) <= Version.Parse(currentVersion))
            return false;

        var http = _httpClientFactory.CreateClient();

        return await http.IsUrlValidAsync(
            await GetDownloadUrlAsync(latestVersion, cToken),
            cToken
        );
    }

    /// <inheritdoc/>
    public bool RemoveOldFiles()
    {
        var result = false;

        foreach (var file in Directory.GetFiles(AppContext.BaseDirectory).Where(x => x.EndsWith(OldFileSuffix, StringComparison.Ordinal)))
            result |= KotzUtilities.TryDeleteFile(file);

        return result;
    }

    /// <inheritdoc/>
    public async ValueTask<string> GetLatestVersionAsync(CancellationToken cToken = default)
    {
        try
        {
            return (await GetLatestVersionFromApiAsync(cToken)).Tag;
        }
        catch (InvalidOperationException)
        {
            return await GetLatestVersionFromUrlAsync(cToken);
        }
    }

    /// <inheritdoc/>
    public async ValueTask<(string? OldVersion, string? NewVersion)> InstallOrUpdateAsync(string installationUri, CancellationToken cToken = default)
    {
        var currentVersion = await GetCurrentVersionAsync(cToken);
        var latestVersion = await GetLatestVersionAsync(cToken);

        if (currentVersion is not null && Version.Parse(latestVersion) <= Version.Parse(currentVersion))
            return (currentVersion, null);

        var http = _httpClientFactory.CreateClient();   // Do not initialize a GithubClient here, it returns 302 with no data
        var appTempLocation = Path.Join(_tempDirectory, _downloadedFileName[.._downloadedFileName.LastIndexOf('.')]);
        var zipTempLocation = Path.Join(_tempDirectory, _downloadedFileName);

        try
        {
            await using var downloadStream = await http.GetStreamAsync(
                await GetDownloadUrlAsync(latestVersion, cToken),
                cToken
            );

            // Save the zip file
            await using (var fileStream = new FileStream(zipTempLocation, FileMode.Create))
                await downloadStream.CopyToAsync(fileStream, cToken);

            // Extract the zip file
            await Task.Run(() => ZipFile.ExtractToDirectory(zipTempLocation, _tempDirectory), cToken);

            // Move the new binary and its dependencies
            var newFilesUris = Directory.EnumerateFiles(appTempLocation);

            foreach (var newFileUri in newFilesUris)
            {
                var destinationUri = Path.Join(AppContext.BaseDirectory, newFileUri[(newFileUri.LastIndexOf(Path.DirectorySeparatorChar) + 1)..]);

                // Rename the original file from "file" to "file_old".
                if (File.Exists(destinationUri))
                {
                    if (Environment.OSVersion.Platform is not PlatformID.Unix)
                        File.Move(destinationUri, destinationUri + OldFileSuffix, true);
                    else
                    {
                        using var moveProcess = KotzUtilities.StartProcess("mv", [destinationUri, destinationUri + OldFileSuffix]);
                        await moveProcess.WaitForExitAsync(cToken);
                    }
                }
                
                // Move the new file to the application's directory.
                // ...
                // This is a workaround for a really weird bug with Unix applications published as single-file.
                // The moving operation works, but invoking any process from the shell results in:
                // FileNotFoundException: Could not load file or assembly 'System.IO.Pipes, Version=9.0.0.0 [...] 
                if (Environment.OSVersion.Platform is not PlatformID.Unix)
                    File.Move(newFileUri, destinationUri, true);
                else
                {
                    using var moveProcess = KotzUtilities.StartProcess("mv", [newFileUri, destinationUri]);
                    await moveProcess.WaitForExitAsync(cToken);
                }
            }

            // Mark the new binary file as executable.c
            if (Environment.OSVersion.Platform is PlatformID.Unix)
            {
                using var chmod = KotzUtilities.StartProcess("chmod", ["+x", BinaryUri]);
                await chmod.WaitForExitAsync(cToken);
            }

            return (currentVersion, latestVersion);
        }
        finally
        {
            // Cleanup
            KotzUtilities.TryDeleteFile(zipTempLocation);
            KotzUtilities.TryDeleteDirectory(appTempLocation);
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
            Architecture.X64 when OperatingSystem.IsWindows() => "NadekoHub_win-x64.zip",
            Architecture.Arm64 when OperatingSystem.IsWindows() => "NadekoHub_win-arm64.zip",

            // Linux
            Architecture.X64 when OperatingSystem.IsLinux() => "NadekoHub_linux-x64.zip",
            Architecture.Arm64 when OperatingSystem.IsLinux() => "NadekoHub_linux-arm64.zip",

            // MacOS
            Architecture.X64 when OperatingSystem.IsMacOS() => "NadekoHub_osx-x64.zip",
            Architecture.Arm64 when OperatingSystem.IsMacOS() => "NadekoHub_osx-arm64.zip",
            _ => throw new NotSupportedException($"Architecture of type {RuntimeInformation.OSArchitecture} is not supported by NadekoHub on this OS.")
        };
    }

    /// <summary>
    /// Gets the download url to the latest bot release.
    /// </summary>
    /// <param name="latestVersion">The latest version of the bot.</param>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns>The url to the latest bot release.</returns>
    private async ValueTask<string> GetDownloadUrlAsync(string latestVersion, CancellationToken cToken = default)
    {
        try
        {
            // The first release is the most recent one.
            return (await GetLatestVersionFromApiAsync(cToken)).Assets
                .First(x => x.Name.Equals(_downloadedFileName, StringComparison.Ordinal))
                .Url;
        }
        catch (InvalidOperationException)
        {
            return $"https://github.com/Kaoticz/NadekoHub/releases/download/{latestVersion}/{_downloadedFileName}";
        }
    }

    /// <summary>
    /// Gets the latest bot version from the Gitlab latest release URL.
    /// </summary>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns>The latest version of the bot.</returns>
    /// <exception cref="InvalidOperationException">Occurs when parsing of the response fails.</exception>
    private async ValueTask<string> GetLatestVersionFromUrlAsync(CancellationToken cToken = default)
    {
        var http = _httpClientFactory.CreateClient(AppConstants.GithubClient);
        var response = await http.GetAsync(_githubReleasesRepoUrl, cToken);

        var lastSlashIndex = response.Headers.Location?.OriginalString.LastIndexOf('/')
            ?? throw new InvalidOperationException("Failed to get the latest NadekoHub version.");

        return response.Headers.Location.OriginalString[(lastSlashIndex + 1)..];
    }

    /// <summary>
    /// Gets the latest bot version from the Gitlab API.
    /// </summary>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns>The latest version of the bot.</returns>
    /// <exception cref="InvalidOperationException">Occurs when the API call fails.</exception>
    private async ValueTask<GithubRelease> GetLatestVersionFromApiAsync(CancellationToken cToken = default)
    {
        if (_memoryCache.TryGetValue(_cachedCurrentVersionKey, out var cachedObject) && cachedObject is GithubRelease cachedResponse)
            return cachedResponse;

        var http = _httpClientFactory.CreateClient(AppConstants.GithubClient);
        var httpResponse = await http.GetAsync(_githubReleasesEndpointUrl, cToken);

        if (!httpResponse.IsSuccessStatusCode)
            throw new InvalidOperationException("The call to the Github API failed.");

        var response = JsonSerializer.Deserialize<GithubRelease>(await httpResponse.Content.ReadAsStringAsync(cToken))
            ?? throw new InvalidOperationException("Failed deserializing Github's response.");

        _memoryCache.Set(_cachedCurrentVersionKey, response, TimeSpan.FromMinutes(1));

        return response;
    }
}