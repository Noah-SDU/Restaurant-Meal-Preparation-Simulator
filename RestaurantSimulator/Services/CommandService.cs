using System;
using System.Collections.Generic;
using System.Linq;

namespace RestaurantSimulator.Services;

public interface IIngredientActions
{
    bool TryBuyIngredient(string name, double amount, out string message, out decimal totalCost);
    bool TryAddIngredient(string name, double amount, out string message);
}

public interface IMoneyActions
{
    decimal Money { get; }
    void RecordExpense(decimal amount);
}

public interface IOrderActions
{
    bool AreAllIngredientsComplete();
    bool TryAddIngredient(string ingredientName, double amount, out string message);
}

public interface IStationActions
{
    bool TryStartStep(string stepName, string stationName, out string message);
}

public interface ICommandHandler
{
    string Name { get; }
    string Description { get; }
    string Execute(string[] args);
}

public class CommandService
{
    public const string ClearConsoleSignal = "CLEAR_CONSOLE";
    
    private readonly Dictionary<string, ICommandHandler> _commands = new();

    private readonly IIngredientActions? _ingredients;
    private readonly IStationActions? _stations;
    private readonly IMoneyActions? _money;
    private readonly IOrderActions? _orders;

    public CommandService(
        IIngredientActions? ingredients = null,
        IStationActions? stations = null,
        IMoneyActions? money = null,
        IOrderActions? orders = null)
    {
        _ingredients = ingredients;
        _stations = stations;
        _money = money;
        _orders = orders;
        RegisterDefaultCommands();
    }

    public void RegisterCommand(ICommandHandler handler)
    {
        _commands[handler.Name.ToLowerInvariant()] = handler;
    }

    public string ExecuteCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var parts = Tokenize(input);
        if (parts.Length == 0)
        {
            return string.Empty;
        }
        var commandName = parts[0].ToLowerInvariant();
        var args = parts.Skip(1).ToArray();

        // Check if this is a step assignment command (quoted step name followed by "at")
        /*if (parts.Length >= 3 && parts[0].StartsWith("\"") && _stationsActions != null)
        {
            try
            {
                // Check if all ingredients are complete before allowing step assignment
                if (_ordersActions != null && !_ordersActions.AreAllIngredientsComplete())
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
                            if (_stationsActions.TryStartStep(stepName, stationName, out var message))
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
        }*/

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
    
    private static string[] Tokenize(string input)
    {
        var tokens = new List<string>();
        var current = "";
        var inQuotes = false;

        foreach (var ch in input.Trim())
        {
            if (ch == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (!inQuotes && char.IsWhiteSpace(ch))
            {
                if (current.Length > 0)
                {
                    tokens.Add(current);
                    current = "";
                }
                continue;
            }

            current += ch;
        }

        if (current.Length > 0)
            tokens.Add(current);

        return tokens.ToArray();
    }


    private void RegisterDefaultCommands()
    {
        RegisterCommand(new HelpCommand(_commands));
        RegisterCommand(new ClearCommand());
        
        if (_ingredients != null && _money != null)
            RegisterCommand(new BuyCommand(_ingredients, _money));
        if (_ingredients != null && _orders != null)
            RegisterCommand(new AddCommand(_ingredients, _orders));
        if (_stations != null && _orders != null)
            RegisterCommand(new AssignStepCommand(_stations, _orders));
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
        helpText += "  step            - step \"<step name>\" at <station name>\n";
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
        return CommandService.ClearConsoleSignal;
    }
}

public class BuyCommand : ICommandHandler
{
    private readonly IIngredientActions _ingredients;
    private readonly IMoneyActions _money;

    public string Name => "buy";
    public string Description => "buy <ingredient name> <amount>";

    public BuyCommand(IIngredientActions ingredients, IMoneyActions money)
    {
        _ingredients = ingredients;
        _money = money;
    }

    public string Execute(string[] args)
    {
        if (args.Length < 2)
            return "Usage: buy <ingredient name> <amount>";

        var amountToken = args[^1];
        var ingredientName = string.Join(" ", args.Take(args.Length - 1));

        if (!double.TryParse(amountToken, out var amount) || amount <= 0)
            return "Invalid amount. Must be a positive number.";

        if (!_ingredients.TryBuyIngredient(ingredientName, amount, out var message, out var totalCost))
            return message;

        _money.RecordExpense(totalCost);
        return $"{message}. Money left: {_money.Money:C2}";
    }
}

public class AddCommand : ICommandHandler
{
    private readonly IIngredientActions _ingredients;
    private readonly IOrderActions _orders;

    public string Name => "add";
    public string Description => "add <ingredient name> <amount>";

    public AddCommand(IIngredientActions ingredients, IOrderActions orders)
    {
        _ingredients = ingredients;
        _orders = orders;
    }

    public string Execute(string[] args)
    {
        if (args.Length < 2)
            return "Usage: add <ingredient name> <amount>";

        var amountToken = args[^1];
        var ingredientName = string.Join(" ", args.Take(args.Length - 1));

        if (!double.TryParse(amountToken, out var amount) || amount <= 0)
            return "Invalid amount. Must be a positive number.";

        if (!_ingredients.TryAddIngredient(ingredientName, amount, out var message))
            return message;

        if (_orders.TryAddIngredient(ingredientName, amount, out var recipeMessage))
            return recipeMessage;

        return message;
    }
}

public class AssignStepCommand : ICommandHandler
{
    private readonly IStationActions _stations;
    private readonly IOrderActions _orders;

    public string Name => "step";
    public string Description => "step \"<step name>\" at <station name>";

    public AssignStepCommand(IStationActions stations, IOrderActions orders)
    {
        _stations = stations;
        _orders = orders;
    }

    public string Execute(string[] args)
    {
        if (args.Length < 3)
            return "Usage: step \"<step name>\" at <station name>";

        if (!_orders.AreAllIngredientsComplete())
            return "Cannot assign steps yet. All ingredients must be added to the recipe first.";

        var atIndex = Array.FindIndex(args, a => a.Equals("at", StringComparison.OrdinalIgnoreCase));
        if (atIndex <= 0 || atIndex == args.Length - 1)
            return "Usage: step \"<step name>\" at <station name>";

        var stepName = string.Join(" ", args.Take(atIndex));
        var stationName = string.Join(" ", args.Skip(atIndex + 1));

        if (string.IsNullOrWhiteSpace(stepName) || string.IsNullOrWhiteSpace(stationName))
            return "Usage: step \"<step name>\" at <station name>";

        return _stations.TryStartStep(stepName, stationName, out var message) ? message : message;
    }
}