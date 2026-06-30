namespace Accyourate.App.EnterpriseSearch;

public interface ISearchProvider
{
    string ProviderId { get; }
    string DisplayName { get; }
    IEnumerable<SearchResult> Search(string query);
}
