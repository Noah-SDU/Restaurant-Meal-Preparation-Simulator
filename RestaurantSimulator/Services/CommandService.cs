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
    private readonly OrdersViewModel? _ordersViewModel;

    public CommandService(
        IngredientsViewModel? ingredientsViewModel = null,
        StationsViewModel? stationsViewModel = null,
        MoneyViewModel? moneyViewModel = null,
        OrdersViewModel? ordersViewModel = null)
    {
        _ingredientsViewModel = ingredientsViewModel;
        _stationsViewModel = stationsViewModel;
        _moneyViewModel = moneyViewModel;
        _ordersViewModel = ordersViewModel;
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

        // Check if this is a step assignment command (quoted step name followed by "at")
        if (parts.Length >= 3 && parts[0].StartsWith("\"") && _stationsViewModel != null)
        {
            try
            {
                // Check if all ingredients are complete before allowing step assignment
                if (_ordersViewModel != null && !_ordersViewModel.AreAllIngredientsComplete())
                    return "Cannot assign steps yet. All ingredients must be added to the recipe first.";

                // Find the closing quote
                var fullInput = input.Trim();
                var firstQuoteIndex = fullInput.IndexOf('"');
                var secondQuoteIndex = fullInput.IndexOf('"', firstQuoteIndex + 1);
                
                if (firstQuoteIndex >= 0 && secondQuoteIndex > firstQuoteIndex)
                {
                    var stepName = fullInput.Substring(firstQuoteIndex + 1, secondQuoteIndex - firstQuoteIndex - 1);
                    var afterQuotes = fullInput.Substring(secondQuoteIndex + 1).Trim();
                    
                    if (afterQuotes.StartsWith("at ", StringComparison.OrdinalIgnoreCase))
                    {
                        var stationName = afterQuotes.Substring(3).Trim();
                        
                        if (!string.IsNullOrWhiteSpace(stationName))
                        {
                            if (_stationsViewModel.TryStartStep(stepName, stationName, out var message))
                                return message;
                            return message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error executing step command: {ex.Message}";
            }
        }

        // Try to execute as a registered command
        if (_commands.TryGetValue(commandName, out var handler))
        {
            try
            {
                return handler.Execute(args);
            }
            catch (Exception ex)
            {
                return $"Error executing command: {ex.Message}";
            }
        }

        return $"Unknown command: '{commandName}'. Type 'help' for available commands.";
    }

    private void RegisterDefaultCommands()
    {
        RegisterCommand(new HelpCommand(_commands));
        RegisterCommand(new ClearCommand());
        if (_ingredientsViewModel != null && _moneyViewModel != null)
            RegisterCommand(new BuyCommand(_ingredientsViewModel, _moneyViewModel));
        if (_ingredientsViewModel != null && _ordersViewModel != null)
            RegisterCommand(new AddCommand(_ingredientsViewModel, _ordersViewModel));
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
        helpText += "  step            - \"<step name>\" at <station name>\n";
        helpText += "\n[IMPORTANT] All ingredients must be added to the recipe BEFORE steps can be assigned to stations.\n";
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

        if (!double.TryParse(amountToken, out var amount) || amount <= 0)
            return "Invalid amount. Must be a positive number.";

        if (!_ingredientsViewModel.TryBuyIngredient(ingredientName, amount, out var message, out var totalCost, out _))
            return message;

        _moneyViewModel.RecordExpense(totalCost);
        return $"{message}. Money left: {_moneyViewModel.Money:C2}";
    }
}

public class AddCommand : ICommandHandler
{
    private readonly IngredientsViewModel _ingredientsViewModel;
    private readonly OrdersViewModel _ordersViewModel;

    public string Name => "add";
    public string Description => "add <ingredient name> <amount>";

    public AddCommand(IngredientsViewModel ingredientsViewModel, OrdersViewModel ordersViewModel)
    {
        _ingredientsViewModel = ingredientsViewModel;
        _ordersViewModel = ordersViewModel;
    }

    public string Execute(string[] args)
    {
        if (args.Length < 2)
            return "Usage: add <ingredient name> <amount>";

        var amountToken = args[^1];
        var ingredientName = string.Join(" ", args.Take(args.Length - 1));

        if (!double.TryParse(amountToken, out var amount) || amount <= 0)
            return "Invalid amount. Must be a positive number.";

        if (!_ingredientsViewModel.TryAddIngredient(ingredientName, amount, out var message, out var unit))
            return message;

        // Now add to the recipe progress
        if (_ordersViewModel.TryAddIngredient(ingredientName, amount, out var recipeMessage))
            return recipeMessage;

        return message;
    }
}
