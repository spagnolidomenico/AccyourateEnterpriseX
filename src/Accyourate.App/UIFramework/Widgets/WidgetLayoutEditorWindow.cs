using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.UIFramework.Widgets;

public sealed class WidgetLayoutEditorWindow : Window
{
    private readonly WorkspaceWidgetLayout _layout;
    private readonly StackPanel _list = new();

    public WidgetLayoutEditorWindow(WorkspaceWidgetLayout layout)
    {
        _layout = layout;

        Title = "Personalizza Workspace";
        Width = 820;
        Height = 760;
        MinWidth = 680;
        MinHeight = 620;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();
        Refresh();
    }

    private Control BuildLayout()
    {
        var page = new StackPanel { Margin = new Thickness(28), Spacing = 18 };

        page.Children.Add(new TextBlock
        {
            Text = "Personalizza widget",
            FontSize = 30,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        page.Children.Add(new TextBlock
        {
            Text = "Seleziona i widget da visualizzare nella Control Room. Il layout viene salvato per utente.",
            TextWrapping = TextWrapping.Wrap,
            Foreground = UiTokens.Brush(UiTokens.TextSecondary)
        });

        page.Children.Add(new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            CornerRadius = new CornerRadius(22),
            Padding = new Thickness(20),
            Child = new ScrollViewer
            {
                Content = _list,
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
            }
        });

        var save = new Button
        {
            Content = "Salva layout",
            Background = UiTokens.Brush(UiTokens.BrandBlue),
            Foreground = Brushes.White,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(14, 10),
            CornerRadius = new CornerRadius(14)
        };
        save.Click += (_, _) =>
        {
            WorkspaceWidgetStorage.Save(_layout);
            Close();
        };

        page.Children.Add(save);
        return page;
    }

    private void Refresh()
    {
        _list.Children.Clear();
        _list.Spacing = 8;

        foreach (var widget in WorkspaceWidgetRegistry.Widgets)
        {
            var check = new CheckBox
            {
                Content = $"{widget.Icon}  {widget.Title}    —    {widget.Category}",
                IsChecked = _layout.VisibleWidgetIds.Contains(widget.Id),
                Foreground = UiTokens.Brush(UiTokens.TextPrimary),
                FontSize = 15
            };

            check.IsCheckedChanged += (_, _) =>
            {
                if (check.IsChecked == true)
                {
                    if (!_layout.VisibleWidgetIds.Contains(widget.Id))
                        _layout.VisibleWidgetIds.Add(widget.Id);
                }
                else
                {
                    _layout.VisibleWidgetIds.Remove(widget.Id);
                }
            };

            _list.Children.Add(check);
        }
    }
}
