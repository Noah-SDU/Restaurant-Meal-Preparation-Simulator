using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantSimulator.ViewModels;

namespace RestaurantSimulator.Services;

public interface ICommandHandler
{
    string Name { get; }
    string Description { get; }
    string Execute(string[] args);
}

public class CommandService
{
    private readonly Dictionary<string, ICommandHandler> _commands = new();

    private readonly IngredientsViewModel? _ingredientsViewModel;
    private readonly StationsViewModel? _stationsViewModel;
    private readonly MoneyViewModel? _moneyViewModel;

    public CommandService(
        IngredientsViewModel? ingredientsViewModel = null,
        StationsViewModel? stationsViewModel = null,
        MoneyViewModel? moneyViewModel = null)
    {
        _ingredientsViewModel = ingredientsViewModel;
        _stationsViewModel = stationsViewModel;
        _moneyViewModel = moneyViewModel;
        RegisterDefaultCommands();
    }

    public void RegisterCommand(ICommandHandler handler)
    {
        _commands[handler.Name.ToLower()] = handler;
    }

    public string ExecuteCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var parts = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var commandName = parts[0].ToLower();
        var args = parts.Skip(1).ToArray();

        if (!_commands.TryGetValue(commandName, out var handler))
        {
            if (input.Contains(" to ", StringComparison.OrdinalIgnoreCase) && _commands.TryGetValue("move", out var moveHandler))
                return moveHandler.Execute(parts);

            return $"Unknown command: '{commandName}'. Type 'help' for available commands.";
        }

        try
        {
            return handler.Execute(args);
        }
        catch (Exception ex)
        {
            return $"Error executing command: {ex.Message}";
        }
    }

    private void RegisterDefaultCommands()
    {
        RegisterCommand(new HelpCommand(_commands));
        RegisterCommand(new ClearCommand());
        if (_ingredientsViewModel != null && _stationsViewModel != null)
            RegisterCommand(new MoveCommand(_ingredientsViewModel, _stationsViewModel));
        if (_ingredientsViewModel != null && _moneyViewModel != null)
            RegisterCommand(new BuyCommand(_ingredientsViewModel, _moneyViewModel));
    }
}

public class HelpCommand : ICommandHandler
{
    private readonly Dictionary<string, ICommandHandler> _commands;

    public string Name => "help";
    public string Description => "Show available commands";

    public HelpCommand(Dictionary<string, ICommandHandler> commands)
    {
        _commands = commands;
    }

    public string Execute(string[] args)
    {
        var helpText = "Available commands:\n";
        foreach (var cmd in _commands.Values.OrderBy(c => c.Name))
        {
            helpText += $"  {cmd.Name,-15} - {cmd.Description}\n";
        }
        return helpText;
    }
}

public class ClearCommand : ICommandHandler
{
    public string Name => "clear";
    public string Description => "Clear the console";

    public string Execute(string[] args)
    {
        return "CLEAR_CONSOLE"; // Special return value to signal clearing
    }
}

public class MoveCommand : ICommandHandler
{
    private readonly IngredientsViewModel _ingredientsViewModel;
    private readonly StationsViewModel _stationsViewModel;

    public string Name => "move";
    public string Description => "<ingredient name> <amount> to <station name>";

    public MoveCommand(IngredientsViewModel ingredientsViewModel, StationsViewModel stationsViewModel)
    {
        _ingredientsViewModel = ingredientsViewModel;
        _stationsViewModel = stationsViewModel;
    }

    public string Execute(string[] args)
    {
        if (args.Length < 3)
            return "Usage: <ingredient name> <amount> to <station name>";

        // Find "to" keyword
        var toIndex = Array.FindIndex(args, arg => arg.Equals("to", StringComparison.OrdinalIgnoreCase));
        if (toIndex == -1 || toIndex < 2)
            return "Usage: <ingredient name> <amount> to <station name>";

        // Extract ingredient name (everything before the amount)
        string ingredientName = string.Join(" ", args.Take(toIndex - 1));
        string amountStr = args[toIndex - 1];
        string stationName = string.Join(" ", args.Skip(toIndex + 1));

        // Parse amount
        if (!int.TryParse(amountStr, out var amount) || amount <= 0)
            return "Invalid amount. Must be a positive number.";

        // Find station
        var station = _stationsViewModel.Stations.FirstOrDefault(s =>
            s.Type.Equals(stationName, StringComparison.OrdinalIgnoreCase));
        if (station == null)
            return $"Station '{stationName}' not found.";

        if (!station.CanAccept(ingredientName))
            return $"Station '{station.Type}' is at capacity for ingredient types. Capasity: {station.DefaultCount}, Types used: {station.DistinctIngredientTypes}";

        // Move ingredient stock
        if (!_ingredientsViewModel.TryMoveIngredient(ingredientName, amount, out var message, out var unit))
            return message;

        // Record moved ingredient in station UI data.
        if (!_stationsViewModel.TryRecordIngredientMove(station.Type, ingredientName, amount, unit, out var stationMessage))
            return stationMessage;

        return $"{message} to {station.Type}";
    }
}

public class BuyCommand : ICommandHandler
{
    private readonly IngredientsViewModel _ingredientsViewModel;
    private readonly MoneyViewModel _moneyViewModel;

    public string Name => "buy";
    public string Description => "buy <ingredient name> <amount>";

    public BuyCommand(IngredientsViewModel ingredientsViewModel, MoneyViewModel moneyViewModel)
    {
        _ingredientsViewModel = ingredientsViewModel;
        _moneyViewModel = moneyViewModel;
    }

    public string Execute(string[] args)
    {
        if (args.Length < 2)
            return "Usage: buy <ingredient name> <amount>";

        var amountToken = args[^1];
        var ingredientName = string.Join(" ", args.Take(args.Length - 1));

        if (!int.TryParse(amountToken, out var amount) || amount <= 0)
            return "Invalid amount. Must be a positive number.";

        if (!_ingredientsViewModel.TryBuyIngredient(ingredientName, amount, out var message, out var totalCost, out _))
            return message;

        _moneyViewModel.RecordExpense(totalCost);
        return $"{message}. Money left: {_moneyViewModel.Money:C2}";
    }
}
