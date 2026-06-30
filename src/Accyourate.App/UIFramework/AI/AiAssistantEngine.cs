using Accyourate.App.Data;

namespace Accyourate.App.UIFramework.AI;

public sealed class AiAssistantEngine
{
    private readonly AiDataQueryService _queryService;
    private readonly AiRoutingEngine _routingEngine = new();

    public AiAssistantEngine(DatabaseService database)
    {
        _queryService = new AiDataQueryService(database);
    }

    public AiAssistantIntent Analyze(string? query)
    {
        var originalQuery = query ?? string.Empty;

        if (string.IsNullOrWhiteSpace(originalQuery))
        {
            return Intent(
                originalQuery,
                "Guida",
                "Scrivi una domanda o scegli uno dei suggerimenti.",
                "Puoi chiedere informazioni su dispositivi, Digital Twin, ECG, documenti, asset, dashboard, analytics o moduli del gestionale.");
        }

        var matches = _routingEngine.Match(originalQuery);
        var best = matches.FirstOrDefault();

        if (best is null)
        {
            return Intent(
                originalQuery,
                "Generale",
                "Usa la Command Palette o la ricerca globale.",
                "Non ho riconosciuto un modulo specifico. Puoi provare con parole come Digital Twin, ECG, documenti, asset, KPI, branding o manutenzione.");
        }

        var explanation = BuildExplanation(best, matches);

        return Intent(
            originalQuery,
            best.Intent.Category,
            best.Intent.SuggestedAction,
            explanation);
    }

    private string BuildExplanation(AiRouteMatch best, IReadOnlyList<AiRouteMatch> matches)
    {
        var baseText = best.Intent.Id switch
        {
            "digital-twin" => "Ho riconosciuto un intento Digital Twin. Puoi visualizzare capi tessili intelligenti, telemetria, ECG, battito cardiaco, batteria, RFID/NFC e lifecycle dispositivo.",
            "medical" => $"Ho riconosciuto un intento Medical. {_queryService.CountEntity("medical").Summary}",
            "documents" => $"Ho riconosciuto un intento Documentale. {_queryService.CountEntity("documents").Summary}",
            "assets" => $"Ho riconosciuto un intento Asset IT. {_queryService.CountEntity("assets").Summary}",
            "quality" => $"Ho riconosciuto un intento Qualità. {_queryService.CountEntity("quality").Summary}",
            "maintenance" => $"Ho riconosciuto un intento Manutenzione. {_queryService.CountEntity("maintenance").Summary}",
            "analytics" => "Ho riconosciuto un intento Analytics. Puoi visualizzare KPI, trend operativi e sintesi stato direttamente nella Workspace.",
            "branding" => "Ho riconosciuto un intento Branding. Puoi personalizzare nome azienda, immagine hero, logo e identità visiva.",
            _ => "Ho riconosciuto un intento operativo."
        };

        var confidence = best.Score >= 8 ? "Alta" : best.Score >= 4 ? "Media" : "Bassa";
        var terms = best.MatchedTerms.Length > 0 ? string.Join(", ", best.MatchedTerms) : "nessun termine specifico";
        var alternatives = matches.Skip(1).Take(2).Select(m => m.Intent.Category).ToList();

        if (alternatives.Count > 0)
            return $"{baseText}\n\nConfidenza: {confidence} · Termini riconosciuti: {terms}.\nPossibili alternative: {string.Join(", ", alternatives)}.";

        return $"{baseText}\n\nConfidenza: {confidence} · Termini riconosciuti: {terms}.";
    }

    private static AiAssistantIntent Intent(string query, string category, string suggestedAction, string explanation)
    {
        return new AiAssistantIntent
        {
            Query = query,
            Category = category,
            SuggestedAction = suggestedAction,
            Explanation = explanation
        };
    }
}
