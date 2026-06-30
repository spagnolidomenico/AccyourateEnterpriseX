using Accyourate.App.Infrastructure.Logging;

namespace Accyourate.App.Infrastructure.Errors;

public sealed class ErrorHandler
{
    private readonly AppLogger _logger;

    public ErrorHandler(AppLogger logger)
    {
        _logger = logger;
    }

    public string Handle(Exception exception, string context)
    {
        _logger.Error($"Errore in {context}", exception);
        return $"Si è verificato un errore in {context}. Dettagli registrati nel log.";
    }
}
