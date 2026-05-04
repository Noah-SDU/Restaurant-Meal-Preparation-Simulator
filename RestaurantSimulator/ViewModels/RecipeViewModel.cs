using System.Collections.ObjectModel;
using System.Collections.Generic;
using RestaurantSimulator.Models;

namespace RestaurantSimulator.ViewModels;

public class RecipeViewModel : ViewModelBase
{
    private ObservableCollection<Recipes> _recipes = new();

    public ObservableCollection<Recipes> Recipes
    {
        get => _recipes;
        set => SetProperty(ref _recipes, value);
    }

    public RecipeViewModel(IEnumerable<Recipes> recipes)
    {
        Recipes = new ObservableCollection<Recipes>(recipes);
    }
}
