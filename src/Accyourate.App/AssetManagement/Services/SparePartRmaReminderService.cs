using Accyourate.App.AssetManagement.Models;
using Accyourate.App.Platform.Notifications;
using Microsoft.Data.Sqlite;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SparePartRmaReminderService
{
    private readonly string _databasePath;
    private readonly SparePartRmaRepository _repository;
    private readonly NotificationService _notifications;

    public SparePartRmaReminderService(string? databasePath=null,NotificationService? notifications=null)
    {
        var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"AccyourateEnterpriseX");
        Directory.CreateDirectory(folder);
        _databasePath=databasePath??Path.Combine(folder,"accyourate-assets.db");
        _repository=new SparePartRmaRepository(_databasePath);
        _notifications=notifications??new NotificationService();
        using var connection=Open();using var command=connection.CreateCommand();command.CommandText="""
            CREATE TABLE IF NOT EXISTS SparePartRmaNotificationLog(
                NotificationKey TEXT PRIMARY KEY,
                RmaId INTEGER NOT NULL,
                CreatedAt TEXT NOT NULL
            );
            """;command.ExecuteNonQuery();
    }

    public int PublishDueNotifications()
    {
        var count=0;
        foreach(var item in _repository.GetAll().Where(IsActive))
        {
            if(!DateTime.TryParse(item.DueDate,out var due))continue;
            var days=(due.Date-DateTime.Today).Days;
            if(days<0)
            {
                count+=PublishOnce($"rma:{item.Id}:overdue:{due:yyyyMMdd}",item,"RMA scaduta",$"La pratica {item.CaseNumber} è scaduta da {Math.Abs(days)} giorni.",NotificationPriority.Critical);
            }
            else if(days<=7)
            {
                count+=PublishOnce($"rma:{item.Id}:due:{due:yyyyMMdd}",item,"RMA in scadenza",$"La pratica {item.CaseNumber} scade tra {days} giorni.",NotificationPriority.High);
            }

            if(item.Status==SparePartRmaStatus.Shipped&&days<=7)
                count+=PublishOnce($"rma:{item.Id}:waiting:{due:yyyyMMdd}",item,"RMA spedita senza esito",$"La pratica {item.CaseNumber}, tracking {Dash(item.TrackingNumber)}, è ancora in attesa dell'esito del fornitore.",NotificationPriority.High);
        }
        return count;
    }

    private int PublishOnce(string key,SparePartRmaCase item,string title,string message,string priority)
    {
        using var connection=Open();using var transaction=connection.BeginTransaction();
        using var check=connection.CreateCommand();check.Transaction=transaction;check.CommandText="SELECT COUNT(*) FROM SparePartRmaNotificationLog WHERE NotificationKey=$key;";check.Parameters.AddWithValue("$key",key);
        if(Convert.ToInt32(check.ExecuteScalar())>0){transaction.Rollback();return 0;}
        _notifications.Publish(title,message,NotificationCategory.Asset,priority,"RMA fornitori","open-rma",item.CaseNumber);
        using var insert=connection.CreateCommand();insert.Transaction=transaction;insert.CommandText="INSERT INTO SparePartRmaNotificationLog(NotificationKey,RmaId,CreatedAt) VALUES($key,$id,$date);";insert.Parameters.AddWithValue("$key",key);insert.Parameters.AddWithValue("$id",item.Id);insert.Parameters.AddWithValue("$date",DateTime.Now.ToString("s"));insert.ExecuteNonQuery();transaction.Commit();return 1;
    }

    private SqliteConnection Open(){var connection=new SqliteConnection($"Data Source={_databasePath}");connection.Open();return connection;}
    private static bool IsActive(SparePartRmaCase item)=>item.Status is not (SparePartRmaStatus.Closed or SparePartRmaStatus.Cancelled);
    private static string Dash(string value)=>string.IsNullOrWhiteSpace(value)?"non disponibile":value;
}
