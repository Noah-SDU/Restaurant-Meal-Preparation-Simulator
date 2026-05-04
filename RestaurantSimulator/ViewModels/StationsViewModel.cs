using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using RestaurantSimulator.Models;

namespace RestaurantSimulator.ViewModels;

public class StationsViewModel : ViewModelBase
{
    private ObservableCollection<StationDisplayViewModel> _stations = new();

    public ObservableCollection<StationDisplayViewModel> Stations
    {
        get => _stations;
        set => SetProperty(ref _stations, value);
    }

    public StationsViewModel(IEnumerable<Stations> stations)
    {
        Stations = new ObservableCollection<StationDisplayViewModel>(
            stations.Select(s => new StationDisplayViewModel
            {
                Type = s.Type,
                DefaultCount = s.DefaultCount
            }));
    }

    public bool ContainsStation(string stationType)
    {
        return Stations.Any(s => s.Type.Equals(stationType, System.StringComparison.OrdinalIgnoreCase));
    }

    public bool TryRecordIngredientMove(string stationType, string ingredientName, int amount, string unit, out string message)
    {
        var station = Stations.FirstOrDefault(s =>
            s.Type.Equals(stationType, System.StringComparison.OrdinalIgnoreCase));

        if (station == null)
        {
            message = $"Station '{stationType}' not found.";
            return false;
        }

        if (!station.CanAccept(ingredientName))
        {
            message = $"Station '{station.Type}' is at capacity for ingredient types. DefaultCount: {station.DefaultCount}, Types used: {station.DistinctIngredientTypes}";
            return false;
        }

        station.AddOrUpdateIngredient(ingredientName, amount, unit);
        message = $"Added {amount} {unit} of {ingredientName} to {station.Type}";
        return true;
    }
}

public class StationDisplayViewModel : ViewModelBase
{
    private string _type = string.Empty;
    private int _capacity;
    private ObservableCollection<StationIngredientViewModel> _movedIngredients = new();

    public string Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    public int DefaultCount
    {
        get => _capacity;
        set => SetProperty(ref _capacity, value);
    }

    public int DistinctIngredientTypes => MovedIngredients.Count;

    public bool CanAccept(string ingredientName)
    {
        return MovedIngredients.Any(i => i.Name.Equals(ingredientName, System.StringComparison.OrdinalIgnoreCase))
            || DistinctIngredientTypes < DefaultCount;
    }

    public ObservableCollection<StationIngredientViewModel> MovedIngredients
    {
        get => _movedIngredients;
        set => SetProperty(ref _movedIngredients, value);
    }

    public void AddOrUpdateIngredient(string ingredientName, int amount, string unit)
    {
        var existing = MovedIngredients.FirstOrDefault(i =>
            i.Name.Equals(ingredientName, System.StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            existing.Amount += amount;
            return;
        }

        MovedIngredients.Add(new StationIngredientViewModel
        {
            Name = ingredientName,
            Amount = amount,
            Unit = unit
        });
    }
}

public class StationIngredientViewModel : ViewModelBase
{
    private string _name = string.Empty;
    private int _amount;
    private string _unit = string.Empty;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public int Amount
    {
        get => _amount;
        set => SetProperty(ref _amount, value);
    }

    public string Unit
    {
        get => _unit;
        set => SetProperty(ref _unit, value);
    }
}
