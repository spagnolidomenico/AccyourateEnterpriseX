using Accyourate.Domain.MasterData;

namespace Accyourate.Core.Repositories;

public interface ICompanyRepository : IRepository<Company>
{
    IReadOnlyList<Company> Search(string query);
}
