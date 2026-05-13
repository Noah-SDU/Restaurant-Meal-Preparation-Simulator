using System;
using System.ComponentModel;
using RestaurantSimulator.ViewModels;

namespace RestaurantSimulator.Services;

public class NavigationService : INavigationService
{
    private readonly MainWindowViewModel _host;
    private readonly Func<Type, ViewModelBase> _viewModelFactory;
    public event PropertyChangedEventHandler? PropertyChanged;
    public NavigationService(MainWindowViewModel host, Func<Type, ViewModelBase> viewModelFactory)
    {
        _host = host;
        _viewModelFactory = viewModelFactory;
    }
    public ViewModelBase? CurrentViewModel => _host.CurrentViewModel;


    public void Navigate(ViewModelBase viewModel)
    {
        _host.CurrentViewModel = viewModel;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentViewModel)));
    }

    public void Navigate<TViewModel>() where TViewModel : ViewModelBase
    {
        var vm = (TViewModel)_viewModelFactory(typeof(TViewModel));
        Navigate(vm);
    }
}
