using Accyourate.Domain.MasterData;

namespace Accyourate.Core.Repositories;

public interface ISupplierRepository : IRepository<Supplier>
{
    IReadOnlyList<Supplier> Search(string query);
    IReadOnlyList<Supplier> GetByCategory(string category);
}
