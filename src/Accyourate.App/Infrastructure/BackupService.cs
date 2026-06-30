using Accyourate.App.Data;

namespace Accyourate.App.Infrastructure;

public sealed class BackupService
{
    private readonly DatabaseService _database;

    public BackupService(DatabaseService database)
    {
        _database = database;
    }

    public string CreateBackup(string createdBy)
    {
        var backupDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Accyourate Enterprise X",
            "backups");

        Directory.CreateDirectory(backupDir);

        var fileName = $"accyourate_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";
        var destination = Path.Combine(backupDir, fileName);

        File.Copy(AppPaths.DatabasePath, destination, overwrite: false);

        _database.WriteAudit(createdBy, "BACKUP_CREATED", destination);
        return destination;
    }

    public List<FileInfo> GetBackups()
    {
        var backupDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Accyourate Enterprise X",
            "backups");

        Directory.CreateDirectory(backupDir);

        return Directory.GetFiles(backupDir, "*.db")
            .Select(x => new FileInfo(x))
            .OrderByDescending(x => x.CreationTime)
            .ToList();
    }
}
