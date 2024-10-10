using System.Text;

namespace Ethik.Utility.Api.Extensions;

/// <summary>
/// Extension methods for HttpClient to support retry logic with logging for GET, POST, PUT, and DELETE operations.
/// </summary>
public static class HttpClientExtensions
{
    private const int DefaultRetries = 3;
    private const int DefaultDelay = 1000;

    /// <summary>
    /// Sends a GET request to the specified URL with retry logic and logging.
    /// </summary>
    /// <param name="client">The HttpClient instance.</param>
    /// <param name="url">The URL to send the GET request to.</param>
    /// <param name="retry">The number of retry attempts (default is 3).</param>
    /// <param name="delay">The delay between retries in milliseconds (default is 1000ms).</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The response content as a string.</returns>
    /// <exception cref="HttpRequestException">Thrown if the request fails after all retries.</exception>
    public static async Task<string> GetWithRetryAsync(this HttpClient client, string url, int retry = DefaultRetries, int delay = DefaultDelay, CancellationToken cancellationToken = default)
    {
        for (int i = 0; i < retry; i++)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (HttpRequestException)
            {
                if (i == retry - 1)
                {
                    throw;
                }
            }
            await Task.Delay(delay, cancellationToken);
        }
        throw new InvalidOperationException("This should never be reached.");
    }

    /// <summary>
    /// Sends a POST request to the specified URL with retry logic and logging.
    /// </summary>
    /// <param name="client">The HttpClient instance.</param>
    /// <param name="url">The URL to send the POST request to.</param>
    /// <param name="jsonData">The JSON data to be sent in the POST request body.</param>
    /// <param name="retry">The number of retry attempts (default is 3).</param>
    /// <param name="delay">The delay between retries in milliseconds (default is 1000ms).</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The response content as a string.</returns>
    /// <exception cref="HttpRequestException">Thrown if the request fails after all retries.</exception>
    public static async Task<string> PostWithRetryAsync(this HttpClient client, string url, string jsonData, int retry = DefaultRetries, int delay = DefaultDelay, CancellationToken cancellationToken = default)
    {
        for (int i = 0; i < retry; i++)
        {
            try
            {
                var stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, stringContent, cancellationToken);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (HttpRequestException)
            {
                if (i == retry - 1)
                {
                    throw;
                }
            }
            await Task.Delay(delay, cancellationToken);
        }
        throw new InvalidOperationException("This should never be reached.");
    }

    /// <summary>
    /// Sends a PUT request to the specified URL with retry logic and logging.
    /// </summary>
    /// <param name="client">The HttpClient instance.</param>
    /// <param name="url">The URL to send the PUT request to.</param>
    /// <param name="jsonData">The JSON data to be sent in the PUT request body.</param>
    /// <param name="retry">The number of retry attempts (default is 3).</param>
    /// <param name="delay">The delay between retries in milliseconds (default is 1000ms).</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The response content as a string.</returns>
    /// <exception cref="HttpRequestException">Thrown if the request fails after all retries.</exception>
    public static async Task<string> PutWithRetryAsync(this HttpClient client, string url, string jsonData, int retry = DefaultRetries, int delay = DefaultDelay, CancellationToken cancellationToken = default)
    {
        for (int i = 0; i < retry; i++)
        {
            try
            {
                var stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync(url, stringContent, cancellationToken);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (HttpRequestException)
            {
                if (i == retry - 1)
                {
                    throw;
                }
            }
            await Task.Delay(delay, cancellationToken);
        }
        throw new InvalidOperationException("This should never be reached.");
    }

    /// <summary>
    /// Sends a DELETE request to the specified URL with retry logic and logging.
    /// </summary>
    /// <param name="client">The HttpClient instance.</param>
    /// <param name="url">The URL to send the DELETE request to.</param>
    /// <param name="retry">The number of retry attempts (default is 3).</param>
    /// <param name="delay">The delay between retries in milliseconds (default is 1000ms).</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The response content as a string.</returns>
    /// <exception cref="HttpRequestException">Thrown if the request fails after all retries.</exception>
    public static async Task<string> DeleteWithRetryAsync(this HttpClient client, string url, int retry = DefaultRetries, int delay = DefaultDelay, CancellationToken cancellationToken = default)
    {
        for (int i = 0; i < retry; i++)
        {
            try
            {
                HttpResponseMessage response = await client.DeleteAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (HttpRequestException)
            {
                if (i == retry - 1)
                {
                    throw;
                }
            }
            await Task.Delay(delay, cancellationToken);
        }
        throw new InvalidOperationException("This should never be reached.");
    }
}