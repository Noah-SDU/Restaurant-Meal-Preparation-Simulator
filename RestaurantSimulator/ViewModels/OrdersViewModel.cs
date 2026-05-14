using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RestaurantSimulator.Models;
using RestaurantSimulator.Services;

namespace RestaurantSimulator.ViewModels;

public enum OrderStepStatus
{
    Pending,
    InProgress,
    Completed
}

public class IngredientProgressViewModel : ViewModelBase
{
    private double _addedQuantity;
    public string Name { get; set; } = "";
    public double RequiredQuantity { get; set; }
    public string Unit { get; set; } = "";

    public double AddedQuantity
    {
        get => _addedQuantity;
        set => SetProperty(ref _addedQuantity, value);
    }

    public bool IsComplete => AddedQuantity >= RequiredQuantity;
}

public class OrderStepViewModel : ViewModelBase
{
    private readonly Step _step;
    private readonly double _multiplier;
    private OrderStepStatus _status = OrderStepStatus.Pending;
    private string _stationName = string.Empty;
    private int _remainingSeconds;

    public Step Step => _step;

    public string Name => _step.Name;

    public string StationType => _step.StationType;

    public int Duration => (int)Math.Ceiling((_step.Duration / 2.0) * _multiplier);

    public OrderStepStatus Status
    {
        get => _status;
        set 
        { 
            if (SetProperty(ref _status, value))
            {
                OnPropertyChanged(nameof(BackgroundColor));
                OnPropertyChanged(nameof(IsCompleted));
                OnPropertyChanged(nameof(IsInProgress));
            }
        }
    }

    public string StationName
    {
        get => _stationName;
        set => SetProperty(ref _stationName, value);
    }

    public int RemainingSeconds
    {
        get => _remainingSeconds;
        set => SetProperty(ref _remainingSeconds, value);
    }

    public bool IsCompleted => Status == OrderStepStatus.Completed;
    public bool IsInProgress => Status == OrderStepStatus.InProgress;

    public string BackgroundColor => IsCompleted ? "#FF69B4" : "#5B3B4B";

    public OrderStepViewModel(Step step, double multiplier)
    {
        _step = step;
        _multiplier = multiplier <= 0 ? 1.0 : multiplier;
    }

    public void Start(string stationName)
    {
        StationName = stationName;
        RemainingSeconds = Duration * 60;
        Status = OrderStepStatus.InProgress;
    }

    public void TickRemainingSeconds(int value)
    {
        RemainingSeconds = value;
    }

    public void Complete()
    {
        RemainingSeconds = 0;
        Status = OrderStepStatus.Completed;
    }
}

public class OrdersViewModel : ViewModelBase, IOrderActions, IDisposable
{
    private readonly List<Recipe> _availableRecipes;
    private readonly Dictionary<string, string> _ingredientUnits;
    private readonly MoneyViewModel? _moneyViewModel;
    private readonly IGameSettings _settings;
    private Recipe? _currentOrder;
    private int _remainingSeconds = 0;
    private CancellationTokenSource? _cancellationTokenSource;
    private Random _random = new();
    private ObservableCollection<OrderStepViewModel> _currentOrderSteps = new();
    private ObservableCollection<IngredientProgressViewModel> _ingredientProgress = new();
    
     public int OrderIntervalSeconds
     {
         get
         {
             var baseInterval = 180 * _settings.TimeMultiplier;
             if (CurrentOrder != null)
             {
                 var difficultyMultiplier = _settings.GetDifficultyMultiplier(CurrentOrder.Difficulty);
                 baseInterval *= difficultyMultiplier;
             }
             return (int)Math.Ceiling(baseInterval);
         }
     }

    public Recipe? CurrentOrder
    {
        get => _currentOrder;
        set
        {
            if (SetProperty(ref _currentOrder, value))
            {
                RefreshCurrentOrderSteps();
                RefreshIngredientProgress();
            }
        }
    }

    public int RemainingSeconds
    {
        get => _remainingSeconds;
        set => SetProperty(ref _remainingSeconds, value);
    }

    public ObservableCollection<OrderStepViewModel> CurrentOrderSteps
    {
        get => _currentOrderSteps;
        set => SetProperty(ref _currentOrderSteps, value);
    }

    public ObservableCollection<IngredientProgressViewModel> IngredientProgress
    {
        get => _ingredientProgress;
        set => SetProperty(ref _ingredientProgress, value);
    }

    public OrdersViewModel(IEnumerable<Recipe> recipes, IEnumerable<IngredientDefinition>? ingredients = null, MoneyViewModel? moneyViewModel = null, IGameSettings? settings = null)
    {
        _moneyViewModel = moneyViewModel;
        _settings = settings ?? new GameSettings();
        _availableRecipes = new List<Recipe>(recipes);
        
        // Build ingredient units lookup
        _ingredientUnits = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (ingredients != null)
        {
            foreach (var ingredient in ingredients)
            {
                _ingredientUnits[ingredient.Name] = ingredient.Unit ?? "";
            }
        }

        CurrentOrder = CreateRandomOrder();

        // Start background task for generating orders
        StartOrderGeneration();
    }

    private void StartOrderGeneration()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _ = GenerateOrdersAsync(_cancellationTokenSource.Token);
        _ = TimerTickAsync(_cancellationTokenSource.Token);
    }

    private async Task GenerateOrdersAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Initial delay of 2 minutes
            RemainingSeconds = OrderIntervalSeconds;
            await Task.Delay(OrderIntervalSeconds * 1000, cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                CurrentOrder = CreateRandomOrder();
                RemainingSeconds = OrderIntervalSeconds;

                // Wait 2 minutes before next order
                await Task.Delay(OrderIntervalSeconds * 1000, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Task was cancelled, normal shutdown
        }
    }

    private async Task TimerTickAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Update every second
                await Task.Delay(1000, cancellationToken);
                
                if (RemainingSeconds > 0)
                {
                    RemainingSeconds--;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Task was cancelled, normal shutdown
        }
    }

    private Recipe? CreateRandomOrder()
    {
        if (_availableRecipes.Count == 0)
            return null;

        var selectedRecipe = _availableRecipes[_random.Next(_availableRecipes.Count)];

        var recipe = new Recipe
        {
            Name = selectedRecipe.Name,
            Difficulty = selectedRecipe.Difficulty,
            SalePrice = selectedRecipe.SalePrice,
            RequiredIngredients = selectedRecipe.RequiredIngredients
                .Select(ing => new RequiredIngredient
                {
                    Name = ing.Name,
                    Quantity = ing.Quantity,
                    Unit = _ingredientUnits.TryGetValue(ing.Name, out var unit) ? unit : ""
                })
                .ToList(),

            Steps = selectedRecipe.Steps
                .Select(s => new Step
                {
                    Name = s.Name,
                    Duration = s.Duration,
                    StationType = s.StationType
                })
                .ToList()
        };

        return recipe;
    }

    private void RefreshCurrentOrderSteps()
    {
        if (CurrentOrder == null)
        {
            CurrentOrderSteps = new ObservableCollection<OrderStepViewModel>();
            return;
        }
        
        var multiplier =
            _settings.TimeMultiplier *
            _settings.GetDifficultyMultiplier(CurrentOrder.Difficulty);

        CurrentOrderSteps = new ObservableCollection<OrderStepViewModel>(
            CurrentOrder.Steps.Select(step => new OrderStepViewModel(step, multiplier)));
    }

    private void RefreshIngredientProgress()
    {
        if (CurrentOrder == null)
        {
            IngredientProgress = new ObservableCollection<IngredientProgressViewModel>();
            return;
        }

        IngredientProgress = new ObservableCollection<IngredientProgressViewModel>(
            CurrentOrder.RequiredIngredients.Select(ing => new IngredientProgressViewModel
            {
                Name = ing.Name,
                RequiredQuantity = ing.Quantity,
                Unit = ing.Unit,
                AddedQuantity = 0
            }));
    }

    public bool TryStartStep(string stepName, string stationName, out OrderStepViewModel? orderStep, out string message)
    {
        orderStep = null;

        if (CurrentOrder == null)
        {
            message = "No active order is available.";
            return false;
        }

        orderStep = CurrentOrderSteps.FirstOrDefault(step =>
            step.Name.Equals(stepName, StringComparison.OrdinalIgnoreCase) &&
            step.StationType.Equals(stationName, StringComparison.OrdinalIgnoreCase));

        if (orderStep == null)
        {
            message = $"Step '{stepName}' was not found for station '{stationName}'.";
            return false;
        }

        if (orderStep.Status == OrderStepStatus.Completed)
        {
            message = $"Step '{orderStep.Name}' is already complete.";
            return false;
        }

        if (orderStep.Status == OrderStepStatus.InProgress)
        {
            message = $"Step '{orderStep.Name}' is already in progress at {orderStep.StationName}.";
            return false;
        }

        orderStep.Start(stationName);
        message = $"Started '{orderStep.Name}' at {stationName}.";
        return true;
    }

    public bool TryAddIngredient(string ingredientName, double amount, out string message)
    {
        if (CurrentOrder == null)
        {
            message = "No active order is available.";
            return false;
        }

        var ingredientProgress = IngredientProgress.FirstOrDefault(ip =>
            ip.Name.Equals(ingredientName, StringComparison.OrdinalIgnoreCase));

        if (ingredientProgress == null)
        {
            message = $"Ingredient '{ingredientName}' is not required for this recipe.";
            return false;
        }

        if (amount <= 0)
        {
            message = "Amount must be a positive number.";
            return false;
        }

        ingredientProgress.AddedQuantity += amount;
        var remaining = Math.Max(0, ingredientProgress.RequiredQuantity - ingredientProgress.AddedQuantity);
        
        if (ingredientProgress.IsComplete)
        {
            message = $"Added {amount} {ingredientProgress.Unit} of {ingredientName}. Complete! (Added {ingredientProgress.AddedQuantity}/{ingredientProgress.RequiredQuantity})";
        }
        else
        {
            message = $"Added {amount} {ingredientProgress.Unit} of {ingredientName}. Progress: {ingredientProgress.AddedQuantity}/{ingredientProgress.RequiredQuantity} {ingredientProgress.Unit} (need {remaining} more)";
        }

        // Check if recipe is now complete
        CheckAndCompleteRecipe();

        return true;
    }

    public void CompleteStep(OrderStepViewModel orderStep)
    {
        orderStep.Complete();
        CheckAndCompleteRecipe();
    }

    public bool AreAllIngredientsComplete()
    {
        if (CurrentOrder == null || IngredientProgress.Count == 0)
            return false;

        return IngredientProgress.All(ing => ing.IsComplete);
    }

    public bool AreAllStepsComplete()
    {
        if (CurrentOrder == null || CurrentOrderSteps.Count == 0)
            return false;

        return CurrentOrderSteps.All(step => step.Status == OrderStepStatus.Completed);
    }

    public bool IsRecipeComplete()
    {
        return AreAllIngredientsComplete() && AreAllStepsComplete();
    }

    private void AdvanceToNextOrder()
    {
        StopOrderGeneration();

        CurrentOrder = CreateRandomOrder();
        RemainingSeconds = OrderIntervalSeconds;

        if (CurrentOrder != null)
        {
            StartOrderGeneration();
        }
    }

    private void CheckAndCompleteRecipe()
    {
        if (!IsRecipeComplete() || CurrentOrder == null)
            return;

        if (_moneyViewModel != null)
        {
            _moneyViewModel.RecordEarning((decimal)CurrentOrder.SalePrice);
        }

        AdvanceToNextOrder();
    }

    public void StopOrderGeneration()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }

    public void Dispose()
    {
        StopOrderGeneration();
    }
}
