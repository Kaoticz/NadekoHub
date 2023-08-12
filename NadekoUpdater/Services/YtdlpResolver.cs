using System.Runtime.InteropServices;

namespace NadekoUpdater.Services;

/// <summary>
/// Service that checks, downloads, installs, and updates yt-dlp.
/// </summary>
public sealed class YtdlpResolver
{
    private const string _ytdlpProcessName = "yt-dlp";
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// The name of the yt-dlp binary file.
    /// </summary>
    public string YtdlpFileName { get; } = GetFileName();

    /// <summary>
    /// Creates a service that checks, downloads, installs, and updates yt-dlp.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    public YtdlpResolver(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    /// <summary>
    /// Installs or updates yt-dlp to this system.
    /// </summary>
    /// <param name="dependenciesUri">The absolute path to the directory where yt-dlp should be installed to.</param>
    /// <returns>
    /// A tuple that may or may not contain the old and new versions of yt-dlp. <br />
    /// (<see langword="string"/>, <see langword="null"/>): No operation was performed. <br />
    /// (<see langword="null"/>, <see langword="string"/>): Yt-dlp was installed. <br />
    /// (<see langword="string"/>, <see langword="string"/>): Yt-dlp was updated.
    /// </returns>
    public async ValueTask<(string? OldVersion, string? NewVersion)> InstallOrUpdateYtdlpAsync(string dependenciesUri)
    {
        var currentVersion = await GetCurrentVersionAsync();
        var newVersion = await GetLatestVersionAsync();

        // Update
        if (currentVersion is not null)
        {
            // If the versions are the same, exit.
            if (currentVersion == newVersion)
                return (currentVersion, null);

            using var ytdlp = Utilities.StartProcess(_ytdlpProcessName, "-U");
            await ytdlp.WaitForExitAsync();

            return (currentVersion, newVersion);
        }

        // Install
        var ytdlpUri = Path.Combine(dependenciesUri, YtdlpFileName);
        var downloadLink = $"https://github.com/yt-dlp/yt-dlp/releases/download/{newVersion}/{YtdlpFileName}";
        using var http = _httpClientFactory.CreateClient();
        using var downloadStream = await http.GetStreamAsync(downloadLink);
        using var fileStream = new FileStream(Path.Combine(dependenciesUri, YtdlpFileName), FileMode.Create);

        await downloadStream.CopyToAsync(fileStream);

        // Update environment variables
        Environment.SetEnvironmentVariable("PATH", $"{Environment.GetEnvironmentVariable("PATH")};{ytdlpUri}", EnvironmentVariableTarget.User);

        return (null, newVersion);
    }

    /// <summary>
    /// Gets the version of yt-dlp current installed on this system.
    /// </summary>
    /// <returns>The version of yt-dlp on this system or <see langword="null"/> if yt-dlp is not installed.</returns>
    public async ValueTask<string?> GetCurrentVersionAsync()
    {
        if (!await Utilities.ProgramExistsAsync(_ytdlpProcessName))
            return null;

        using var ytdlp = Utilities.StartProcess(_ytdlpProcessName, "--version");

        return (await ytdlp.StandardOutput.ReadToEndAsync()).Trim();
    }

    /// <summary>
    /// Gets the latest version of yt-dlp.
    /// </summary>
    /// <returns>The latest version of yt-dlp.</returns>
    /// <exception cref="InvalidOperationException">
    /// Occurs when there is an issue with the redirection of GitHub's latest release link.
    /// </exception>
    public async ValueTask<string> GetLatestVersionAsync()
    {
        using var http = _httpClientFactory.CreateClient(AppStatics.NoRedirectClient);

        var response = await http.GetAsync("https://github.com/yt-dlp/yt-dlp/releases/latest");

        var lastSlashIndex = response.Headers.Location?.OriginalString.LastIndexOf('/')
            ?? throw new InvalidOperationException("Failed to get the latest yt-dlp version.");

        return response.Headers.Location.OriginalString[(lastSlashIndex + 1)..];
    }

    /// <summary>
    /// Gets the name of the yt-dlp binary file to be downloaded.
    /// </summary>
    /// <returns>The name of the yt-dlp binary file.</returns>
    /// <exception cref="NotSupportedException">Occurs when this method is executed in an unsupported platform.</exception>
    private static string GetFileName()
    {
        return RuntimeInformation.OSArchitecture switch
        {
            // Windows
            Architecture.X86 when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "yt-dlp_x86.exe",
            Architecture.X64 when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "yt-dlp.exe",

            // Linux
            Architecture.X86 or Architecture.X64 when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "yt-dlp_linux",
            Architecture.Arm64 when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "yt-dlp_linux_aarch64",

            // MacOS
            Architecture.X86 or Architecture.X64 when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => "yt-dlp_macos_legacy",
            Architecture.Arm64 when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => "yt-dlp_macos",
            _ => throw new NotSupportedException($"Architecture of type {RuntimeInformation.OSArchitecture} is not supported by yt-dlp on this OS.")
        };
    }
}