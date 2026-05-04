using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using RestaurantSimulator.Models;

namespace RestaurantSimulator.ViewModels;

public class IngredientsViewModel : ViewModelBase
{
    private ObservableCollection<Ingredients> _ingredients = new();

    public ObservableCollection<Ingredients> Ingredients
    {
        get => _ingredients;
        set => SetProperty(ref _ingredients, value);
    }

    public IngredientsViewModel(IEnumerable<Ingredients> ingredients)
    {
        Ingredients = new ObservableCollection<Ingredients>(ingredients);
    }

    public bool TryMoveIngredient(string ingredientName, double amount, out string message, out string unit)
    {
        unit = string.Empty;

        var ingredient = Ingredients.FirstOrDefault(i =>
            i.Name.Equals(ingredientName, System.StringComparison.OrdinalIgnoreCase));

        if (ingredient == null)
        {
            message = $"Ingredient '{ingredientName}' not found.";
            return false;
        }

        if (ingredient.InitialStock < amount)
        {
            message = $"Not enough stock. Available: {ingredient.InitialStock}, Requested: {amount}";
            return false;
        }

        ingredient.InitialStock -= amount;
        unit = ingredient.Unit;
        message = $"Moved {amount} {ingredient.Unit} of {ingredient.Name}";

        // Reassign to ensure UI refresh for model types without change notification.
        Ingredients = new ObservableCollection<Ingredients>(Ingredients);
        return true;
    }

    public bool TryBuyIngredient(string ingredientName, double amount, out string message, out decimal totalCost, out string unit)
    {
        totalCost = 0m;
        unit = string.Empty;

        var ingredient = Ingredients.FirstOrDefault(i =>
            i.Name.Equals(ingredientName, System.StringComparison.OrdinalIgnoreCase));

        if (ingredient == null)
        {
            message = $"Ingredient '{ingredientName}' not found.";
            return false;
        }

        if (amount <= 0)
        {
            message = "Amount must be a positive number.";
            return false;
        }

        ingredient.InitialStock += amount;
        unit = ingredient.Unit;
        totalCost = (decimal)amount * (decimal)ingredient.Cost;
        message = $"Bought {amount} {unit} of {ingredient.Name} for {totalCost:C2}";

        Ingredients = new ObservableCollection<Ingredients>(Ingredients);
        return true;
    }

    public bool TryAddIngredient(string ingredientName, double amount, out string message, out string unit)
    {
        unit = string.Empty;

        var ingredient = Ingredients.FirstOrDefault(i =>
            i.Name.Equals(ingredientName, System.StringComparison.OrdinalIgnoreCase));

        if (ingredient == null)
        {
            message = $"Ingredient '{ingredientName}' not found.";
            return false;
        }

        if (amount <= 0)
        {
            message = "Amount must be a positive number.";
            return false;
        }

        ingredient.InitialStock += amount;
        unit = ingredient.Unit;
        message = $"Added {amount} {unit} of {ingredient.Name}";

        Ingredients = new ObservableCollection<Ingredients>(Ingredients);
        return true;
    }
}
