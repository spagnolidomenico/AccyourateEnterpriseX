namespace Accyourate.App.UIFramework.UX;

public static class AxUxGuidelines
{
    public const string ConfirmBeforeDestructiveAction = "Usare AxDialogService.ConfirmAsync prima di eliminazioni, ripristini o operazioni irreversibili.";
    public const string ShowLoadingForLongTasks = "Usare AxLoadingOverlay per backup, restore, generazione PDF e operazioni lente.";
    public const string UseBannersForPersistentFeedback = "Usare AxStatusBanner per messaggi permanenti nella pagina.";
    public const string UseSnackbarForTemporaryFeedback = "Usare AxSnackbar per conferme rapide e non bloccanti.";
}
