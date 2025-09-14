using Blazored.LocalStorage;
using System.Net;
using System.Net.Http.Headers;

namespace ChoreBuddies.Frontend.Features.Authentication;

public class AuthorizedHttpClient(ILocalStorageService localStorage, IAuthService authService) : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage = localStorage;
    private readonly IAuthService _authService = authService;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Get the token from local storage
        var token = await _localStorage.GetItemAsStringAsync("authToken");

        // Add the token to the request header if it exists
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Proceed with the request
        var response = await base.SendAsync(request, cancellationToken);

        // Optional: Handle 401 Unauthorized by logging out
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var refreshSuccess = await _authService.RefreshTokenAsync();

            if (refreshSuccess)
            {
                // Retry the original request with the new token
                token = await _localStorage.GetItemAsStringAsync("authToken");
                if (!string.IsNullOrWhiteSpace(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                response = await base.SendAsync(request, cancellationToken);
            }
        }

        return response;
    }
}
