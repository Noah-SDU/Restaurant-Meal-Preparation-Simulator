using System;
using System.Windows.Input;
using Avalonia.Controls;
using RestaurantSimulator.Models;
using RestaurantSimulator.Services;
using CommunityToolkit.Mvvm.Input;

namespace RestaurantSimulator.ViewModels;

public partial class GameViewModel : ViewModelBase, IDisposable
{
    private readonly CommandService _commandService;
    private readonly INavigationService? _navigationService;
    private readonly IRestaurantDataService _dataService;
    private readonly IGameSettings _settings;
    private string _commandInput = string.Empty;
    private string _consoleOutput = "> Welcome to the Restaurant Meal Preparation Simulator!\n> Type 'help' for available commands.\n";
    private IngredientsViewModel _ingredientsViewModel;
    private StationsViewModel _stationsViewModel;
    private MoneyViewModel _moneyViewModel;
    private OrdersViewModel? _ordersViewModel;

    public string CommandInput
    {
        get => _commandInput;
        set => SetProperty(ref _commandInput, value);
    }

    public string ConsoleOutput
    {
        get => _consoleOutput;
        set => SetProperty(ref _consoleOutput, value);
    }

    public IngredientsViewModel IngredientsViewModel
    {
        get => _ingredientsViewModel;
        set => SetProperty(ref _ingredientsViewModel, value);
    }

    public StationsViewModel StationsViewModel
    {
        get => _stationsViewModel;
        set => SetProperty(ref _stationsViewModel, value);
    }

    public MoneyViewModel MoneyViewModel
    {
        get => _moneyViewModel;
        set => SetProperty(ref _moneyViewModel, value);
    }

    public OrdersViewModel? OrdersViewModel
    {
        get => _ordersViewModel;
        set => SetProperty(ref _ordersViewModel, value);
    }

    public ICommand ExecuteCommandCommand { get; }
    public ICommand ReturnToMenuCommand { get; }

    public GameViewModel(INavigationService? navigationService = null, IRestaurantDataService? dataService = null, IGameSettings? settings = null)
    {
        _navigationService = navigationService;
        _dataService = dataService ?? new DataService("Assets/Recipes.json");
        _settings = settings ?? new GameSettings();
        var data = LoadRestaurantData();
        _ingredientsViewModel = new IngredientsViewModel(data.Ingredients);
        _moneyViewModel = new MoneyViewModel();
        _ordersViewModel = new OrdersViewModel(data.Recipes, data.Ingredients, _moneyViewModel, _settings);
        _stationsViewModel = new StationsViewModel(data.Stations, _ordersViewModel);
        _commandService = new CommandService(_ingredientsViewModel, _stationsViewModel, _moneyViewModel, _ordersViewModel);
        ExecuteCommandCommand = new RelayCommand(_ => ExecuteCommand());
        ReturnToMenuCommand = new RelayCommand(_ => ReturnToMenu());
    }

    private void ReturnToMenu()
    {
        if (_navigationService != null)
        {
            _navigationService.Navigate<MainMenuViewModel>();
        }
        Dispose();
    }

    private RestaurantData LoadRestaurantData()
    {
        try
        {
            return _dataService.ReadRestaurantData(); 
        }
        catch (Exception ex)
        {
            _consoleOutput += $"> Failed to load model data: {ex.Message}\n";
            return new RestaurantData
            {
                Ingredients = new System.Collections.Generic.List<IngredientDefinition>(), 
                Stations = new System.Collections.Generic.List<Station>(),
                Recipes = new System.Collections.Generic.List<Recipe>()
            };
        }
    }

    private void ExecuteCommand()
    {
        if (string.IsNullOrWhiteSpace(CommandInput))
            return;

        // Add command to output
        ConsoleOutput += $"> {CommandInput}\n";

        // Execute command
        var result = _commandService.ExecuteCommand(CommandInput);

        // Handle clear console command
        if (result == CommandService.ClearConsoleSignal)
        {
            ConsoleOutput = string.Empty;
        }
        else if (!string.IsNullOrEmpty(result))
        {
            ConsoleOutput += $"{result}\n";
        }

        // Clear input
        CommandInput = string.Empty;
    }

    public void Dispose()
    {
        _ordersViewModel?.Dispose();
    }
}
