using System;
using CommunityToolkit.Mvvm.Input;
using RestaurantSimulator.Services;


namespace RestaurantSimulator.ViewModels;

public partial class MainMenuViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;

    public MainMenuViewModel(INavigationService navigation)
    {
        _navigation = navigation;
    }

    [RelayCommand]
    private void MainWindow()
    {
        _navigation.Navigate<GameViewModel>();
    }

    [RelayCommand]
    private void Options()
    {
        _navigation.Navigate<OptionsViewModel>();
    }

    [RelayCommand]
    private void Quit()
    {
        Environment.Exit(0);
    }
}
