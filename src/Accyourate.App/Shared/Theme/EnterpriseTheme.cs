using Avalonia.Media;

namespace Accyourate.App.Shared.Theme;

public static class EnterpriseTheme
{
    public const string Primary = "#B5162B";
    public const string Sidebar = "#1F2937";
    public const string SidebarSection = "#9CA3AF";
    public const string SidebarText = "#FFFFFF";
    public const string SidebarMuted = "#D1D5DB";
    public const string SidebarHover = "#374151";
    public const string Workspace = "#F7F7F6";
    public const string Card = "#FFFFFF";
    public const string Success = "#16A34A";
    public const string Warning = "#D97706";
    public const string Danger = "#DC2626";
    public const string Info = "#2563EB";

    public static IBrush PrimaryBrush => Brush.Parse(Primary);
    public static IBrush SidebarBrush => Brush.Parse(Sidebar);
    public static IBrush SidebarTextBrush => Brush.Parse(SidebarText);
    public static IBrush WorkspaceBrush => Brush.Parse(Workspace);
}
