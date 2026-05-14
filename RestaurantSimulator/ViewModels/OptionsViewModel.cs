using System;
using System.Globalization;
using CommunityToolkit.Mvvm.Input;
using RestaurantSimulator.Services;

namespace RestaurantSimulator.ViewModels;

public partial class OptionsViewModel : ViewModelBase
{
    private readonly INavigationService? _navigationService;

    public IGameSettings Settings { get; }
    
    public OptionsViewModel(INavigationService? navigationService = null, IGameSettings? settings = null)
    {
        _navigationService = navigationService;
        Settings = settings ?? new GameSettings();
    }

    [RelayCommand]
    private void ReturnToMenu()
    {
        if (_navigationService != null)
        {
            _navigationService.Navigate<MainMenuViewModel>();
        }
    }

    [RelayCommand]
    private void SetEasyMultiplier(object? parameter)
    {
        SetMultiplier(parameter, v => Settings.EasyMultiplier = v);
    }

    [RelayCommand]
    private void SetMediumMultiplier(object? parameter)
    {
        SetMultiplier(parameter, v => Settings.MediumMultiplier = v);
    }

    [RelayCommand]
    private void SetHardMultiplier(object? parameter)
    {
        SetMultiplier(parameter, v => Settings.HardMultiplier = v);
    }
    
    private static void SetMultiplier(object? parameter, Action<double> setter)
    {
        if (parameter is null) return;

        if (parameter is double d)
        {
            setter(d);
            return;
        }

        if (double.TryParse(parameter.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
            setter(value);
    }
}