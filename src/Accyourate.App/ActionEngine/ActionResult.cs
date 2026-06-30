namespace Accyourate.App.ActionEngine;

public sealed class ActionResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string ModuleId { get; init; } = string.Empty;
    public string SuggestedNavigation { get; init; } = string.Empty;
    public Dictionary<string, string> Data { get; init; } = new();

    public static ActionResult Ok(string message, string moduleId = "", string navigation = "")
    {
        return new ActionResult
        {
            Success = true,
            Message = message,
            ModuleId = moduleId,
            SuggestedNavigation = navigation
        };
    }

    public static ActionResult Fail(string message)
    {
        return new ActionResult
        {
            Success = false,
            Message = message
        };
    }
}
