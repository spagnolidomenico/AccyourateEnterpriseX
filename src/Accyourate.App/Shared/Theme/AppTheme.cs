using Avalonia.Media;

namespace Accyourate.App.Shared.Theme;

public static class AppTheme
{
    public const string PrimaryColor = "#B5162B";
    public const string DarkColor = "#2B2926";
    public const string BackgroundColor = "#F7F7F6";
    public const string CardColor = "#FFFFFF";

    public static IBrush PrimaryBrush => Brush.Parse(PrimaryColor);
    public static IBrush DarkBrush => Brush.Parse(DarkColor);
    public static IBrush BackgroundBrush => Brush.Parse(BackgroundColor);
}
