using System.ComponentModel;
using RestaurantSimulator.ViewModels;

namespace RestaurantSimulator.Services;


public interface INavigationService : INotifyPropertyChanged
{

    ViewModelBase? CurrentViewModel { get; }

    void Navigate(ViewModelBase viewModel);

    void Navigate<TViewModel>() where TViewModel : ViewModelBase;
}
