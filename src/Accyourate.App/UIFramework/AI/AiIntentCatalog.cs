namespace Accyourate.App.UIFramework.AI;

public static class AiIntentCatalog
{
    public static IReadOnlyList<AiIntentDefinition> Intents { get; } = new List<AiIntentDefinition>
    {
        new()
        {
            Id = "digital-twin",
            Category = "Digital Twin",
            ModuleId = "digital-twin",
            SuggestedAction = "Apri Digital Twin Platform nella Workspace.",
            StrongKeywords = new[]
            {
                "digital twin", "gemello digitale", "telemetria", "telemetry", "ecg",
                "elettrocardiogramma", "monitoraggio cardiaco", "battito cardiaco",
                "frequenza cardiaca", "smart textile", "capo tessile", "corpetto intelligente"
            },
            Keywords = new[]
            {
                "cardiaco", "cuore", "battito", "sensore", "sensori", "rfid", "nfc", "qr", "qr code",
                "bluetooth", "wifi", "wi-fi", "firmware", "batteria", "segnale", "qualità segnale",
                "textile", "tessile", "wearable", "indossabile", "top", "maglia", "dispositivo intelligente",
                "log ecg", "vitali", "parametri vitali"
            }
        },
        new()
        {
            Id = "medical",
            Category = "Medical",
            ModuleId = "medical",
            SuggestedAction = "Apri Medical nella Workspace.",
            StrongKeywords = new[] { "medical", "dispositivo medico", "dispositivi medici", "medical suite" },
            Keywords = new[] { "manutenzione dispositivo", "qualità dispositivo", "collaudo medicale", "sanificazione" }
        },
        new()
        {
            Id = "documents",
            Category = "Documentale",
            ModuleId = "documents",
            SuggestedAction = "Apri Document Management o cerca nell'archivio.",
            StrongKeywords = new[] { "documenti", "documento", "manuale", "certificato", "archivio" },
            Keywords = new[] { "allegato", "scadenza documento", "pdf", "file", "manualistica" }
        },
        new()
        {
            Id = "assets",
            Category = "IT Asset",
            ModuleId = "assets",
            SuggestedAction = "Apri Asset IT.",
            StrongKeywords = new[] { "asset", "asset it", "pc", "notebook", "computer" },
            Keywords = new[] { "stampante", "assegnato", "inventario it", "hardware", "software" }
        },
        new()
        {
            Id = "analytics",
            Category = "Analytics",
            ModuleId = "analytics",
            SuggestedAction = "Apri Dashboard o Analytics nella Workspace.",
            StrongKeywords = new[] { "dashboard", "analytics", "kpi", "report", "grafico", "grafici" },
            Keywords = new[] { "trend", "statistiche", "analisi", "andamento", "metriche" }
        },
        new()
        {
            Id = "branding",
            Category = "Branding",
            ModuleId = "branding",
            SuggestedAction = "Apri Branding Center.",
            StrongKeywords = new[] { "branding", "brand", "logo", "tema", "colore", "colori" },
            Keywords = new[] { "immagine", "hero", "splash", "personalizzazione", "azienda" }
        },
        new()
        {
            Id = "quality",
            Category = "Qualità",
            ModuleId = "medical",
            SuggestedAction = "Apri Medical o Analytics per controllare qualità e collaudi.",
            StrongKeywords = new[] { "qualità", "qualita", "test qualità", "test qualita", "conformità", "conformita" },
            Keywords = new[] { "collaudo", "non conformità", "non conformita", "controllo qualità", "controllo qualita" }
        },
        new()
        {
            Id = "maintenance",
            Category = "Manutenzione",
            ModuleId = "medical",
            SuggestedAction = "Apri Medical per visualizzare gli interventi.",
            StrongKeywords = new[] { "manutenzione", "manutenzioni", "intervento", "assistenza", "riparazione" },
            Keywords = new[] { "guasto", "riparare", "ticket tecnico", "service" }
        }
    };
}
