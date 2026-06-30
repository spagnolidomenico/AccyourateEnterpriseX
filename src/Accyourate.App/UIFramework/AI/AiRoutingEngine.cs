namespace Accyourate.App.UIFramework.AI;

public sealed class AiRoutingEngine
{
    public IReadOnlyList<AiRouteMatch> Match(string? query)
    {
        var q = Normalize(query);
        var matches = new List<AiRouteMatch>();

        if (string.IsNullOrWhiteSpace(q))
            return matches;

        foreach (var intent in AiIntentCatalogStorage.Load())
        {
            var score = 0;
            var matched = new List<string>();

            foreach (var term in intent.StrongKeywords)
            {
                if (ContainsTerm(q, term))
                {
                    score += 5;
                    matched.Add(term);
                }
            }

            foreach (var term in intent.Keywords)
            {
                if (ContainsTerm(q, term))
                {
                    score += 2;
                    matched.Add(term);
                }
            }

            if (score > 0)
            {
                matches.Add(new AiRouteMatch
                {
                    Intent = intent,
                    Score = score,
                    MatchedTerms = matched.Distinct().ToArray()
                });
            }
        }

        return matches
            .OrderByDescending(m => m.Score)
            .ThenBy(m => m.Intent.Category)
            .ToList();
    }

    public AiRouteMatch? BestMatch(string? query)
    {
        return Match(query).FirstOrDefault();
    }

    private static string Normalize(string? query)
    {
        return (query ?? string.Empty).Trim().ToLowerInvariant();
    }

    private static bool ContainsTerm(string query, string term)
    {
        return query.Contains(term.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase);
    }
}
