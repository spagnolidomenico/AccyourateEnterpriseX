using Avalonia.Media;
using Avalonia.Styling;
using Accyourate.App.UIFramework.Tokens;
using AvaloniaApplication = global::Avalonia.Application;

namespace Accyourate.App.UIFramework.Foundation;

/// <summary>
/// Central application theme coordinator. It owns the active theme mode,
/// exposes the corresponding semantic palette and applies Avalonia's theme variant.
/// </summary>
public sealed class AxThemeManager
{
    private static readonly Lazy<AxThemeManager> LazyCurrent = new(() => new AxThemeManager());

    private AvaloniaApplication? _application;

    private AxThemeManager()
    {
    }

    public static AxThemeManager Current => LazyCurrent.Value;

    public UiThemeMode Mode { get; private set; } = UiThemeMode.Light;

    public AxThemePalette Palette => AxThemePalette.For(Mode);

    public event EventHandler<UiThemeMode>? ThemeChanged;

    public void Initialize(AvaloniaApplication application, UiThemeMode initialMode = UiThemeMode.Light)
    {
        _application = application ?? throw new ArgumentNullException(nameof(application));
        SetTheme(initialMode, force: true);
    }

    public void SetTheme(UiThemeMode mode) => SetTheme(mode, force: false);

    public UiThemeMode Toggle()
    {
        SetTheme(Mode == UiThemeMode.Light ? UiThemeMode.Dark : UiThemeMode.Light);
        return Mode;
    }

    public static IBrush Brush(string color) => Avalonia.Media.Brush.Parse(color);

    private void SetTheme(UiThemeMode mode, bool force)
    {
        if (!force && Mode == mode)
            return;

        Mode = mode;
        if (_application is not null)
        {
            _application.RequestedThemeVariant = mode == UiThemeMode.Dark
                ? ThemeVariant.Dark
                : ThemeVariant.Light;
        }

        ThemeChanged?.Invoke(this, mode);
    }
}
