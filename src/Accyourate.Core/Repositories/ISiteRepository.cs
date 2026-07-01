using Accyourate.Domain.MasterData;

namespace Accyourate.Core.Repositories;

public interface ISiteRepository : IRepository<Site>
{
    IReadOnlyList<Site> GetByCompanyId(int companyId);
    Site? GetMainSite(int companyId);
    IReadOnlyList<Site> Search(string query);
}
