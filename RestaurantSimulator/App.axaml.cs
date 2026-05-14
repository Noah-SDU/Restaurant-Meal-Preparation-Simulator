using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using System;
using Avalonia.Markup.Xaml;
using RestaurantSimulator.ViewModels;
using RestaurantSimulator.Views;
using RestaurantSimulator.Services;

namespace RestaurantSimulator;

public partial class App : Application
{
    private NavigationService? _navigationService;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainVm = new MainWindowViewModel();
            var settings = new GameSettings();
            var dataService = new DataService();

            ViewModelBase CreateViewModel(Type type)
            {
                // Handle special cases where VMs need the navigation service
                if (type == typeof(MainMenuViewModel))
                    return new MainMenuViewModel(_navigationService!);
                if (type == typeof(GameViewModel))
                    return new GameViewModel(_navigationService!, dataService, settings);
                if (type == typeof(OptionsViewModel))
                    return new OptionsViewModel(_navigationService!, settings);

                // Default factory for other types
                return (ViewModelBase)Activator.CreateInstance(type)!;
            }

            _navigationService = new NavigationService(mainVm, CreateViewModel);

            // Navigate to the initial page
            _navigationService.Navigate<MainMenuViewModel>();

            var mainWindow = new MainWindow()
            {
                DataContext = mainVm,
            };

            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}