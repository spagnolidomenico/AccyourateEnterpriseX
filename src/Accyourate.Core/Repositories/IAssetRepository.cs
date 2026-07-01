using Accyourate.Domain.Assets;

namespace Accyourate.Core.Repositories;

public interface IAssetRepository : IRepository<Asset>
{
    IReadOnlyList<Asset> Search(string query);
    IReadOnlyList<Asset> GetByStatus(string status);
    IReadOnlyList<Asset> GetAvailableForAssignment();
}
