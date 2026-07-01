using Accyourate.Domain.Assets;

namespace Accyourate.Core.Repositories;

public interface IAssetRepository
{
    IReadOnlyList<Asset> GetAll();
    Asset? GetById(int id);
    int Create(Asset asset);
    void Update(Asset asset);
    void Delete(int id);
}
