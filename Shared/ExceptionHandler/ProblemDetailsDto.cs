namespace Shared.ExceptionHandler;

public record ProblemDetailsDto(
    string? Title,
    string? Detail,
    int? Status
);
