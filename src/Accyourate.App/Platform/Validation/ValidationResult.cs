namespace Accyourate.App.Platform.Validation;

public sealed class ValidationResult
{
    private readonly List<ValidationMessage> _messages = new();

    public IReadOnlyList<ValidationMessage> Messages => _messages;
    public IReadOnlyList<ValidationMessage> Errors => _messages.Where(x => x.Severity == ValidationSeverity.Error).ToList();
    public IReadOnlyList<ValidationMessage> Warnings => _messages.Where(x => x.Severity == ValidationSeverity.Warning).ToList();
    public bool IsValid => Errors.Count == 0;

    public void AddError(string field, string code, string message)
    {
        _messages.Add(new ValidationMessage
        {
            Field = field,
            Code = code,
            Message = message,
            Severity = ValidationSeverity.Error
        });
    }

    public void AddWarning(string field, string code, string message)
    {
        _messages.Add(new ValidationMessage
        {
            Field = field,
            Code = code,
            Message = message,
            Severity = ValidationSeverity.Warning
        });
    }

    public string ToDisplayMessage()
    {
        return string.Join(Environment.NewLine, Errors.Select(x => $"• {x.Message}"));
    }
}
