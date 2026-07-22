using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Themes.Fluent;
using Accyourate.App.Data;
using Accyourate.App.Security;
using Accyourate.App.UIFramework.Foundation;

namespace Accyourate.App;

public sealed class App : Avalonia.Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        AxThemeManager.Current.Initialize(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var database = new DatabaseService();
            database.Initialize();

            var auth = new AuthenticationService(database);
            var loginWindow = new BrandedSplashLoginWindow(database, auth);

            loginWindow.LoginSucceeded += user =>
            {
                var mainWindow = new MainWindow(user, database);
                desktop.MainWindow = mainWindow;
                mainWindow.Show();
                loginWindow.Close();
            };

            desktop.MainWindow = loginWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
