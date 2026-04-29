using System;
using System.ComponentModel;
using RestaurantSimulator.ViewModels;

namespace RestaurantSimulator.Services;

public class NavigationService : INavigationService
{
    private readonly MainWindowViewModel _host;
    public event PropertyChangedEventHandler? PropertyChanged;
    public NavigationService(MainWindowViewModel host)
    {
        _host = host;
    }
    public ViewModelBase? CurrentViewModel => _host.CurrentViewModel;


    public void Navigate(ViewModelBase viewModel)
    {
        _host.CurrentViewModel = viewModel;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentViewModel)));
    }

    public void Navigate<TViewModel>() where TViewModel : ViewModelBase
    {
        object? instance;

        var navigationConstructor = typeof(TViewModel).GetConstructor(new[] { typeof(INavigationService) });
        if (navigationConstructor != null)
        {
            instance = navigationConstructor.Invoke(new object[] { this });
        }
        else
        {
            var parameterlessConstructor = typeof(TViewModel).GetConstructor(Type.EmptyTypes);
            if (parameterlessConstructor == null)
            {
                throw new InvalidOperationException(
                    $"Cannot navigate to {typeof(TViewModel).Name}: no usable constructor found.");
            }

            instance = parameterlessConstructor.Invoke(Array.Empty<object>());
        }

        Navigate((TViewModel)instance!);
    }
}
