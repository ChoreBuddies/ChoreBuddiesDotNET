namespace ChoreBuddies.Frontend.Utilities;

public class Result
{
    public bool Succeeded { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static Result Success() => new() { Succeeded = true };
    public static Result Fail(string message) => new() { Succeeded = false, ErrorMessage = message };
}
