using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using RestaurantSimulator.Models;
using RestaurantSimulator.Services;

namespace RestaurantSimulator.ViewModels;

public class IngredientsViewModel : ViewModelBase, IIngredientActions 
{
    private ObservableCollection<IngredientDefinition> _ingredients = new();

    public ObservableCollection<IngredientDefinition> Ingredients
    {
        get => _ingredients;
        set => SetProperty(ref _ingredients, value);
    }

    public IngredientsViewModel(IEnumerable<IngredientDefinition> ingredients)
    {
        Ingredients = new ObservableCollection<IngredientDefinition>(ingredients);
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
        Ingredients = new ObservableCollection<IngredientDefinition>(Ingredients);
        return true;
    }

    public bool TryBuyIngredient(string name, double amount, out string message, out decimal totalCost)
    {
        totalCost = 0m;

        var ingredient = Ingredients.FirstOrDefault(i =>
            i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));

        if (ingredient == null)
        {
            message = $"Ingredient '{name}' not found.";
            return false;
        }

        if (amount <= 0)
        {
            message = "Amount must be a positive number.";
            return false;
        }

        ingredient.InitialStock += amount;
        totalCost = (decimal)amount * (decimal)ingredient.Cost;
        message = $"Bought {amount} {ingredient.Unit} of {ingredient.Name} for {totalCost:C2}";

        Ingredients = new ObservableCollection<IngredientDefinition>(Ingredients);
        return true;
    }

    public bool TryAddIngredient(string name, double amount, out string message)
    {

        var ingredient = Ingredients.FirstOrDefault(i =>
            i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));

        if (ingredient == null)
        {
            message = $"Ingredient '{name}' not found.";
            return false;
        }

        if (amount <= 0)
        {
            message = "Amount must be a positive number.";
            return false;
        }

        ingredient.InitialStock += amount;
        message = $"Added {amount} {ingredient.Unit} of {ingredient.Name}";

        Ingredients = new ObservableCollection<IngredientDefinition>(Ingredients);
        return true;
    }
}
