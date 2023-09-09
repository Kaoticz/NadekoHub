using System.Text.Json;

namespace NadekoHub.Extensions;

/// <summary>
/// Defines extension methods for <see cref="HttpClient"/>.
/// </summary>
public static class HttpClientExt
{
    /// <summary>
    /// Sends a GET request to an API at the specified <paramref name="endpoint"/> and returns a Json deserialized response.
    /// </summary>
    /// <typeparam name="T">The type to be returned.</typeparam>
    /// <param name="http">This http client.</param>
    /// <param name="endpoint">The API endpoint to be called.</param>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns>A <typeparamref name="T"/> response.</returns>
    /// <exception cref="InvalidOperationException">Occurs when the deserialization fails.</exception>
    public static async Task<T> CallApiAsync<T>(this HttpClient http, string endpoint, CancellationToken cToken = default)
    {
        var responseString = await http.GetStringAsync(endpoint, cToken);

        return JsonSerializer.Deserialize<T>(responseString)
            ?? throw new InvalidOperationException($"Could not deserialize response to {nameof(T)}.");
    }

    /// <summary>
    /// Checks if a request to the specified <paramref name="url"/> returns a successful HTTP response.
    /// </summary>
    /// <param name="http">This http client.</param>
    /// <param name="url">The url to check.</param>
    /// <param name="cToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if the <paramref name="url"/> is valid, <see langword="false"/> otherwise.</returns>
    public static async Task<bool> IsUrlValidAsync(this HttpClient http, string url, CancellationToken cToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Head, url);
        var response = await http.SendAsync(request, cToken);

        return response.IsSuccessStatusCode;
    }
}