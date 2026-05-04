using System;
using System.Windows.Input;
using Avalonia.Controls;
using RestaurantSimulator.Models;
using RestaurantSimulator.Services;

namespace RestaurantSimulator.ViewModels;

public partial class GameViewModel : ViewModelBase
{
    private readonly CommandService _commandService;
    private string _commandInput = string.Empty;
    private string _consoleOutput = "> Welcome to the Restaurant Meal Preparation Simulator!\n> Type 'help' for available commands.\n";
    private IngredientsViewModel _ingredientsViewModel;
    private StationsViewModel _stationsViewModel;
    private MoneyViewModel _moneyViewModel;
    private RecipeViewModel _recipeViewModel;

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

    public RecipeViewModel RecipeViewModel
    {
        get => _recipeViewModel;
        set => SetProperty(ref _recipeViewModel, value);
    }

    public ICommand ExecuteCommandCommand { get; }

    public GameViewModel()
    {
        var data = LoadRestaurantData();
        _ingredientsViewModel = new IngredientsViewModel(data.Ingredients);
        _stationsViewModel = new StationsViewModel(data.Stations);
        _moneyViewModel = new MoneyViewModel();
        _recipeViewModel = new RecipeViewModel(data.Recipes);
        _commandService = new CommandService(_ingredientsViewModel, _stationsViewModel, _moneyViewModel);
        ExecuteCommandCommand = new RelayCommand(_ => ExecuteCommand());
    }

    private RestaurantData LoadRestaurantData()
    {
        try
        {
            var dataService = new DataService("Assets/Recipes.json");
            return dataService.ReadRestaurantData();
        }
        catch (Exception ex)
        {
            _consoleOutput += $"> Failed to load model data: {ex.Message}\n";
            return new RestaurantData
            {
                Ingredients = new System.Collections.Generic.List<Ingredients>(),
                Stations = new System.Collections.Generic.List<Stations>(),
                Recipes = new System.Collections.Generic.List<Recipes>()
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
        if (result == "CLEAR_CONSOLE")
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
}

public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;

#pragma warning disable CS0067
    public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) => _execute(parameter);
}
