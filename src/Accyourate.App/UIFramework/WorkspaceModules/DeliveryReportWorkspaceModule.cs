using Avalonia.Controls;
using Accyourate.App.AssetManagement.DeliveryReports;

namespace Accyourate.App.UIFramework.WorkspaceModules;

public sealed class DeliveryReportWorkspaceModule : IWorkspaceModule
{
    public string Id => "delivery-reports";
    public string Title => "Verbali di consegna";
    public string Icon => "📄";
    public bool CanClose => true;
    public bool IsPinned => false;

    public Control CreateView()
    {
        return new DeliveryReportView();
    }
}
