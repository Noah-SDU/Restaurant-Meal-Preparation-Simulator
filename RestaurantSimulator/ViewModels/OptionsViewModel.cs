using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using RestaurantSimulator.Services;

namespace RestaurantSimulator.ViewModels;

public partial class OptionsViewModel : ViewModelBase
{
    private readonly INavigationService? _navigationService;

    public ICommand ReturnToMenuCommand { get; }

    public OptionsViewModel(INavigationService? navigationService = null)
    {
        _navigationService = navigationService;
        ReturnToMenuCommand = new RelayCommand(_ => ReturnToMenu());
    }

    private void ReturnToMenu()
    {
        if (_navigationService != null)
        {
            _navigationService.Navigate<MainMenuViewModel>();
        }
    }
}