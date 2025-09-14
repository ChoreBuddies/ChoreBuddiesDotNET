using Blazored.LocalStorage;
using System.Net;

namespace ChoreBuddies.Frontend.Features.Authentication;

public class AuthorizedHttpClient(ILocalStorageService localStorage) : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage = localStorage;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Get the token from local storage
        var token = await _localStorage.GetItemAsStringAsync("authToken");

        // Add the token to the request header if it exists
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Proceed with the request
        var response = await base.SendAsync(request, cancellationToken);

        // Optional: Handle 401 Unauthorized by logging out
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // You might want to trigger a logout event or try to refresh the token here
        }

        return response;
    }
}
