namespace Accyourate.App.Platform.Validation;

public sealed class ValidationException : Exception
{
    public ValidationResult Result { get; }

    public ValidationException(ValidationResult result)
        : base(result.ToDisplayMessage())
    {
        Result = result;
    }
}
