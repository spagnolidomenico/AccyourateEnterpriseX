using Accyourate.Domain.MasterData;

namespace Accyourate.Core.Repositories;

public interface IDepartmentRepository : IRepository<Department>
{
    IReadOnlyList<Department> GetBySiteId(int siteId);
    IReadOnlyList<Department> Search(string query);
}
