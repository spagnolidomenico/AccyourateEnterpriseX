namespace Accyourate.App.UIFramework.Shell;

public static class ShellRegistry
{
    public static IReadOnlyList<ShellModuleDescriptor> Modules { get; } = new List<ShellModuleDescriptor>
    {
        new() { Id = "home", Title = "Home", Section = "Centro Operativo", Icon = "⌂" },
        new() { Id = "dashboard", Title = "Dashboard", Section = "Centro Operativo", Icon = "▥" },
        new() { Id = "analytics", Title = "Analytics", Section = "Centro Operativo", Icon = "▧" },
        new() { Id = "medical", Title = "Medical Device Suite", Section = "Medical", Icon = "⌁" },
        new() { Id = "documents", Title = "Document Management", Section = "Documentale", Icon = "▤" },
        new() { Id = "assets", Title = "Asset IT", Section = "IT", Icon = "▣" },
        new() { Id = "people", Title = "Persone", Section = "HR", Icon = "👥" },
        new() { Id = "branding", Title = "Branding Center", Section = "Amministrazione", Icon = "🏷" },
        new() { Id = "design-system", Title = "Design System", Section = "Amministrazione", Icon = "🎛" },
        new() { Id = "architecture", Title = "Enterprise Architecture", Section = "Amministrazione", Icon = "🏗" }
    };
}
