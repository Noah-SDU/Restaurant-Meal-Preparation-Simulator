using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System;
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
            Services.NavigationService? navigation = null;
            navigation = new Services.NavigationService(mainVm, type => 
            {
                // Handle special cases where VMs need the navigation service
                if (type == typeof(MainMenuViewModel))
                    return new MainMenuViewModel(navigation);
                if (type == typeof(GameViewModel))
                    return new GameViewModel(navigation);
                // Default factory for other types
                return (ViewModelBase)Activator.CreateInstance(type)!;
            });

            // Navigate to the initial page
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