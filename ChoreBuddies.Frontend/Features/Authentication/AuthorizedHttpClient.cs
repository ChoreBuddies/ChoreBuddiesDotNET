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
        var token = await _localStorage.GetItemAsStringAsync(AuthFrontendConstants.AuthTokenKey);

        request.Headers.Authorization = null;
        // Add the token to the request header if it exists
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue(AuthFrontendConstants.Bearer, token);

        // Proceed with the request
        var response = await base.SendAsync(request, cancellationToken);

        // Optional: Handle 401 Unauthorized by logging out
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var refreshSuccess = await _authService.RefreshTokenAsync();

            if (refreshSuccess)
            {
                // Retry the original request with the new token
                token = await _localStorage.GetItemAsStringAsync(AuthFrontendConstants.AuthTokenKey);

                var newRequest = await CloneHttpRequestMessageAsync(request);

                newRequest.Headers.Authorization = null;
                if (!string.IsNullOrWhiteSpace(token))
                {
                    newRequest.Headers.Authorization = new AuthenticationHeaderValue(AuthFrontendConstants.Bearer, token);
                }

                response = await base.SendAsync(newRequest, cancellationToken);
            }
        }

        return response;
    }

    private async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage req)
    {
        var clone = new HttpRequestMessage(req.Method, req.RequestUri);

        // Copy Content (via a MemoryStream) and headers
        if (req.Content != null)
        {
            var ms = new MemoryStream();
            await req.Content.CopyToAsync(ms);
            ms.Position = 0;
            clone.Content = new StreamContent(ms);

            foreach (var h in req.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(h.Key, h.Value);
        }

        // Copy version and headers
        clone.Version = req.Version;

        foreach (var h in req.Headers)
            clone.Headers.TryAddWithoutValidation(h.Key, h.Value);

        return clone;
    }
}
