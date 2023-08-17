using System.Text.Json;

namespace NadekoUpdater.Extensions;

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
}