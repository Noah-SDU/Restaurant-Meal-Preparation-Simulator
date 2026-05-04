using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using RestaurantSimulator.Models;

namespace RestaurantSimulator.ViewModels;

public class StationsViewModel : ViewModelBase
{
    private ObservableCollection<StationDisplayViewModel> _stations = new();
    private OrdersViewModel? _ordersViewModel;

    public ObservableCollection<StationDisplayViewModel> Stations
    {
        get => _stations;
        set => SetProperty(ref _stations, value);
    }

    public StationsViewModel(IEnumerable<Stations> stations, OrdersViewModel? ordersViewModel = null)
    {
        _ordersViewModel = ordersViewModel;
        Stations = new ObservableCollection<StationDisplayViewModel>(
            stations.Select(s => new StationDisplayViewModel
            {
                Type = s.Type,
                SlotCount = s.DefaultCount,
                OrdersViewModel = _ordersViewModel
            }));
    }

    public void SetOrdersViewModel(OrdersViewModel ordersViewModel)
    {
        _ordersViewModel = ordersViewModel;
        foreach (var station in Stations)
        {
            station.OrdersViewModel = ordersViewModel;
        }
    }

    public bool ContainsStation(string stationType)
    {
        return Stations.Any(s => s.Type.Equals(stationType, System.StringComparison.OrdinalIgnoreCase));
    }

    public bool TryStartStep(string stepName, string stationType, out string message)
    {
        var station = Stations.FirstOrDefault(s =>
            s.Type.Equals(stationType, System.StringComparison.OrdinalIgnoreCase));

        if (station == null)
        {
            message = $"Station '{stationType}' not found.";
            return false;
        }

        if (_ordersViewModel == null)
        {
            message = "No orders view is connected.";
            return false;
        }

        return station.TryStartStep(stepName, _ordersViewModel, out message);
    }
}

public class StationDisplayViewModel : ViewModelBase
{
    private string _type = string.Empty;
    private int _slotCount = 0;
    private OrdersViewModel? _ordersViewModel;
    private ObservableCollection<OrderStepViewModel> _activeSteps = new();
    private Dictionary<OrderStepViewModel, CancellationTokenSource> _stepCancellationTokens = new();

    public string Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    public int SlotCount
    {
        get => _slotCount;
        set => SetProperty(ref _slotCount, value);
    }

    public OrdersViewModel? OrdersViewModel
    {
        get => _ordersViewModel;
        set
        {
            SetProperty(ref _ordersViewModel, value);
        }
    }

    public ObservableCollection<OrderStepViewModel> ActiveSteps
    {
        get => _activeSteps;
        set
        {
            if (SetProperty(ref _activeSteps, value))
            {
                OnPropertyChanged(nameof(HasActiveSteps));
                OnPropertyChanged(nameof(HasNoActiveSteps));
            }
        }
    }

    public bool HasActiveSteps => _activeSteps.Count > 0;

    public bool HasNoActiveSteps => !HasActiveSteps;

    public bool IsBusy => HasActiveSteps;

    public ICommand ReduceTimeCommand { get; }

    public StationDisplayViewModel()
    {
        _activeSteps.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasActiveSteps));
            OnPropertyChanged(nameof(HasNoActiveSteps));
            OnPropertyChanged(nameof(IsBusy));
        };

        ReduceTimeCommand = new RelayCommand<OrderStepViewModel?>(ReduceStepTimeForStep);
    }

    public bool TryStartStep(string stepName, OrdersViewModel ordersViewModel, out string message)
    {
        if (_activeSteps.Count >= SlotCount)
        {
            message = $"Station '{Type}' is at full capacity ({SlotCount} slots).";
            return false;
        }

        if (!ordersViewModel.TryStartStep(stepName, Type, out var orderStep, out message) || orderStep == null)
        {
            return false;
        }

        StartTimer(orderStep, ordersViewModel);
        message = $"Started '{orderStep.Name}' at {Type}.";
        return true;
    }

    private void StartTimer(OrderStepViewModel orderStep, OrdersViewModel ordersViewModel)
    {
        _activeSteps.Add(orderStep);
        orderStep.TickRemainingSeconds(orderStep.Duration * 60);

        var cancellationTokenSource = new CancellationTokenSource();
        _stepCancellationTokens[orderStep] = cancellationTokenSource;
        _ = RunStepTimerAsync(orderStep, ordersViewModel, cancellationTokenSource.Token);
    }

    private async Task RunStepTimerAsync(OrderStepViewModel orderStep, OrdersViewModel ordersViewModel, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && orderStep.RemainingSeconds > 0)
            {
                await Task.Delay(1000, cancellationToken);
                orderStep.TickRemainingSeconds(orderStep.RemainingSeconds - 1);
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                CompleteActiveStep(orderStep, ordersViewModel);
            }
        }
        catch (OperationCanceledException)
        {
            // Task was cancelled, normal shutdown
        }
    }

    private void CompleteActiveStep(OrderStepViewModel orderStep, OrdersViewModel ordersViewModel)
    {
        if (_stepCancellationTokens.TryGetValue(orderStep, out var cancellationTokenSource))
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            _stepCancellationTokens.Remove(orderStep);
        }

        _activeSteps.Remove(orderStep);
        ordersViewModel.CompleteStep(orderStep);
    }

    private void ReduceStepTimeForStep(OrderStepViewModel? step)
    {
        if (step == null || !_activeSteps.Contains(step))
            return;

        if (step.RemainingSeconds <= 0)
            return;

        step.TickRemainingSeconds(Math.Max(0, step.RemainingSeconds - 10));

        if (step.RemainingSeconds == 0 && _ordersViewModel != null)
        {
            CompleteActiveStep(step, _ordersViewModel);
        }
    }
}
