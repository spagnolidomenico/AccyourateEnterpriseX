namespace Accyourate.Shared;

public sealed class Result
{
    private Result(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public bool Success { get; }
    public string Message { get; }

    public static Result Ok(string message = "") => new(true, message);
    public static Result Fail(string message) => new(false, message);
}
