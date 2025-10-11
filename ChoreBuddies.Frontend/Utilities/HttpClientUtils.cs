using System.Net.Http.Json;

namespace ChoreBuddies.Frontend.Utilities;

public class HttpClientUtils(IHttpClientFactory httpClientFactory)
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    private HttpClient CreateClient(bool authorized)
    {
        return _httpClientFactory.CreateClient(authorized ? "AuthorizedClient" : "UnauthorizedClient");
    }

    private async Task<T?> ProcessResponseAsync<T>(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();

        if (typeof(T) == typeof(HttpResponseMessage))
        {
            return (T)(object)response;
        }

        if (response.Content.Headers.ContentLength == 0)
        {
            return default; // Return default value (e.g., null) if there's no content
        }

        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task<T?> GetAsync<T>(string requestUri, bool authorized = false)
    {
        var client = CreateClient(authorized);
        var response = await client.GetAsync(requestUri);
        return await ProcessResponseAsync<T>(response);
    }

    public async Task PostAsync<TRequest>(string requestUri, TRequest content, bool authorized = false)
    {
        var client = CreateClient(authorized);
        var response = await client.PostAsJsonAsync(requestUri, content);
        response.EnsureSuccessStatusCode();
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string requestUri, TRequest content, bool authorized = false)
    {
        var client = CreateClient(authorized);
        var response = await client.PostAsJsonAsync(requestUri, content);
        return await ProcessResponseAsync<TResponse>(response);
    }
    public async Task PutAsync<TRequest>(string requestUri, TRequest content, bool authorized = false)
    {
        var client = CreateClient(authorized);
        var response = await client.PutAsJsonAsync(requestUri, content);
        response.EnsureSuccessStatusCode();
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string requestUri, TRequest content, bool authorized = false)
    {
        var client = CreateClient(authorized);
        var response = await client.PutAsJsonAsync(requestUri, content);
        return await ProcessResponseAsync<TResponse>(response);
    }

    public async Task DeleteAsync(string requestUri, bool authorized = false)
    {
        var client = CreateClient(authorized);
        var response = await client.DeleteAsync(requestUri);
        response.EnsureSuccessStatusCode();
    }
}

public static class HttpClientUtilsExtensions
{
    public static async Task<TRequest?> TryRequestAsync<TRequest>(
        this HttpClientUtils utils,
        Func<Task<TRequest?>> requestFunc,
        Action<HttpRequestException>? onError = null)
    {
        try
        {
            return await requestFunc();
        }
        catch (HttpRequestException ex)
        {
            // It's good practice to at least log the error, even if we're swallowing it.
            Console.Error.WriteLine($"An HTTP request failed: {ex.StatusCode} - {ex.Message}");
            onError?.Invoke(ex);
            return default;
        }
    }
}
