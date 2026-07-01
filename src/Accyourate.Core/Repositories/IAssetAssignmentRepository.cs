using Accyourate.Domain.Assets;

namespace Accyourate.Core.Repositories;

public interface IAssetAssignmentRepository
{
    IReadOnlyList<AssetAssignment> GetActiveByEmployeeId(int employeeId);
    AssetAssignment? GetActiveByAssetId(int assetId);
    int Assign(int assetId, int employeeId, string notes = "");
    void Return(int assignmentId);
}
