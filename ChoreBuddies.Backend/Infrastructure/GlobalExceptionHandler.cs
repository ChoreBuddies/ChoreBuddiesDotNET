using ChoreBuddies.Backend.Features.Households.Exceptions;
using ChoreBuddies.Backend.Infrastructure.Authentication.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ChoreBuddies.Backend.Infrastructure;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unexpected error occurred: {Message}", exception.Message);

        // Default
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred. Please try again later."
        };

        switch (exception)
        {
            case LoginFailedException:
                problemDetails.Status = StatusCodes.Status401Unauthorized;
                problemDetails.Title = "Authentication Failed";
                problemDetails.Detail = exception.Message;
                break;

            case UserAlreadyExistsException:
                problemDetails.Status = StatusCodes.Status409Conflict;
                problemDetails.Title = "Conflict";
                problemDetails.Detail = exception.Message;
                break;

            case RegistrationFailedException:
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Registration Failed";
                problemDetails.Detail = exception.Message;
                break;

            case ArgumentException:
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Bad Request";
                problemDetails.Detail = exception.Message;
                break;
            case InvitationCodeGenerationException:
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Unique Code Genrator Failed";
                problemDetails.Detail = exception.Message;
                break;
            case InvalidInvitationCodeException:
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Joining household failed";
                problemDetails.Detail = exception.Message;
                break;
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
