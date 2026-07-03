using Accyourate.App.HumanResources.Database;
using Accyourate.App.HumanResources.Models;

namespace Accyourate.App.HumanResources.Repositories;

public sealed class EmploymentContractRepository : HrRepositoryBase
{
    public EmploymentContractRepository(HrDatabase? database = null) : base(database) { }

    public IReadOnlyList<EmploymentContract> GetByEmployeeId(int employeeId)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, EmployeeId, ContractType, StartDate, EndDate, JobTitle, Level, Status, Notes
            FROM EmploymentContracts
            WHERE EmployeeId = $EmployeeId
            ORDER BY StartDate DESC;
        """;
        command.Parameters.AddWithValue("$EmployeeId", employeeId);

        using var reader = command.ExecuteReader();
        var result = new List<EmploymentContract>();
        while (reader.Read())
        {
            result.Add(new EmploymentContract
            {
                Id = reader.GetInt32(0),
                EmployeeId = reader.GetInt32(1),
                ContractType = reader.GetString(2),
                StartDate = ReadString(reader, 3),
                EndDate = ReadString(reader, 4),
                JobTitle = ReadString(reader, 5),
                Level = ReadString(reader, 6),
                Status = ReadString(reader, 7),
                Notes = ReadString(reader, 8)
            });
        }
        return result;
    }
}
