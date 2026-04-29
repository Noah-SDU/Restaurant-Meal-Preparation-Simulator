using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using RestaurantSimulator.ViewModels;
using RestaurantSimulator.Views;

namespace RestaurantSimulator;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainVm = new MainWindowViewModel();
            var navigation = new Services.NavigationService(mainVm);

            // Navigate to the initial page using the generic factory method
            navigation.Navigate<MainMenuViewModel>();

            var mainWindow = new MainWindow()
            {
                DataContext = mainVm,
            };

            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}