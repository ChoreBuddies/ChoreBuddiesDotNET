using ChoreBuddies.Frontend.Features.Authentication;
using System.Net.Http.Json;

namespace ChoreBuddies.Frontend.Utilities;

public class HttpClientUtils(IHttpClientFactory httpClientFactory)
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

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
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string requestUri, TRequest content, bool authorized = false)
    {
        var client = CreateClient(authorized);
        var response = await client.PutAsJsonAsync(requestUri, content);
        return await ProcessResponseAsync<TResponse>(response);
    }

    public async Task<HttpResponseMessage> DeleteAsync(string requestUri, bool authorized = false)
    {
        var client = CreateClient(authorized);
        var response = await client.DeleteAsync(requestUri);
        return response;
    }

    private HttpClient CreateClient(bool authorized)
    {
        return _httpClientFactory.CreateClient(authorized ? AuthFrontendConstants.AuthorizedClient : AuthFrontendConstants.UnauthorizedClient);
    }

    private async Task<T?> ProcessResponseAsync<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            return default;
        }

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
            Console.Error.WriteLine($"An HTTP request failed: {ex.StatusCode} - {ex.Message}");
            onError?.Invoke(ex);
            return default;
        }
    }
}
