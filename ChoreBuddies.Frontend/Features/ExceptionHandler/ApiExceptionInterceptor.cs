using ChoreBuddies.Frontend.UI.Services;
using Shared.ExceptionHandler;
using System.Net;
using System.Net.Http.Json;

namespace ChoreBuddies.Frontend.Features.ExceptionHandler;

public class ApiExceptionInterceptor : DelegatingHandler
{
    private readonly ToastService _toastService;

    public ApiExceptionInterceptor(ToastService toastService)
    {
        _toastService = toastService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        await HandleErrorAsync(request, response, cancellationToken);

        return response;
    }

    private async Task HandleErrorAsync(HttpRequestMessage request, HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string title = "Error";
        string message = "Something went wrong.";

        // Special handling for 401 Unauthorized
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // CHECK: Is this a Login request?
            bool isLoginRequest = request.RequestUri != null &&
                                  request.RequestUri.AbsolutePath.Contains("login", StringComparison.OrdinalIgnoreCase);

            if (!isLoginRequest)
            {
                // 401, AuthorizedHttpClient couldn't refresh the token
                Console.WriteLine("401: Unauthorized - session expired");
                _toastService.ShowError("Session Expired", "Please log in again.");
                return;
            }
        }

        try
        {
            // Check if response is JSON
            if (response.Content.Headers.ContentType?.MediaType == "application/json")
            {
                var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>(cancellationToken: cancellationToken);
                if (problemDetails != null)
                {
                    title = problemDetails.Title ?? title;
                    message = problemDetails.Detail ?? message;
                }
            }
            else
            {
                // Fallback for non-JSON errors
                message = await response.Content.ReadAsStringAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(message))
                {
                    message = $"Request failed with status: {(int)response.StatusCode} {response.ReasonPhrase}";
                }
            }
        }
        catch
        {
            message = "An unexpected network error occurred.";
        }

        Console.WriteLine($"API Error: {title} - {message}");
        _toastService.ShowError(title, message);
    }
}
