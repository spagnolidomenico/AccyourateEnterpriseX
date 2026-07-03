using Accyourate.App.HumanResources.Database;
using Accyourate.App.HumanResources.Models;

namespace Accyourate.App.HumanResources.Repositories;

public sealed class EmployeeDocumentRepository : HrRepositoryBase
{
    public EmployeeDocumentRepository(HrDatabase? database = null) : base(database) { }

    public IReadOnlyList<EmployeeDocument> GetByEmployeeId(int employeeId)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, EmployeeId, DocumentType, Title, FilePath, ExpirationDate, UploadedAt, UploadedBy, Notes
            FROM EmployeeDocuments
            WHERE EmployeeId = $EmployeeId
            ORDER BY UploadedAt DESC;
        """;
        command.Parameters.AddWithValue("$EmployeeId", employeeId);

        using var reader = command.ExecuteReader();
        var result = new List<EmployeeDocument>();
        while (reader.Read())
        {
            result.Add(new EmployeeDocument
            {
                Id = reader.GetInt32(0),
                EmployeeId = reader.GetInt32(1),
                DocumentType = ReadString(reader, 2),
                Title = reader.GetString(3),
                FilePath = ReadString(reader, 4),
                ExpirationDate = ReadString(reader, 5),
                UploadedAt = ReadString(reader, 6),
                UploadedBy = ReadString(reader, 7),
                Notes = ReadString(reader, 8)
            });
        }
        return result;
    }
}
