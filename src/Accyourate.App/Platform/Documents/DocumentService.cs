using Accyourate.App.Platform.Audit;
using Accyourate.App.Platform.Notifications;

namespace Accyourate.App.Platform.Documents;

public sealed class DocumentService
{
    private readonly DocumentRepository _repository;
    private readonly AuditService _audit;
    private readonly NotificationService _notifications;

    public DocumentService(DocumentRepository? repository = null, AuditService? audit = null, NotificationService? notifications = null)
    {
        _repository = repository ?? new DocumentRepository();
        _audit = audit ?? new AuditService();
        _notifications = notifications ?? new NotificationService();
    }

    public int Register(DocumentRecord document)
    {
        var id = _repository.Register(document);

        _audit.Track(AuditAction.Created, $"Registrato documento {document.Title}", "Document", id.ToString(), document.Title, document.CreatedBy, AuditSeverity.Info, "DocumentCenter");
        _notifications.Publish("Documento registrato", $"{document.Title} è stato aggiunto al Centro Documenti.", NotificationCategory.Documents, NotificationPriority.Info, document.CreatedBy, "open-document", id.ToString());

        return id;
    }

    public int RegisterFile(string filePath, string title, string category, string relatedEntityType = "", string relatedEntityId = "", string relatedEntityLabel = "", string createdBy = "System", string notes = "")
    {
        return Register(new DocumentRecord
        {
            Title = title,
            Category = category,
            FilePath = filePath,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            RelatedEntityLabel = relatedEntityLabel,
            CreatedBy = createdBy,
            Notes = notes,
            CreatedAt = DateTime.Now.ToString("s")
        });
    }

    public IReadOnlyList<DocumentRecord> GetLatest(int limit = 100) => _repository.GetLatest(limit);

    public IReadOnlyList<DocumentRecord> Search(string query, string category = "", int limit = 100)
    {
        if (string.IsNullOrWhiteSpace(query) && (string.IsNullOrWhiteSpace(category) || category == "Tutti"))
            return _repository.GetLatest(limit);
        return _repository.Search(query ?? string.Empty, category, limit);
    }
}
