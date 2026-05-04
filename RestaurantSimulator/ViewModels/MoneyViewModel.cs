namespace RestaurantSimulator.ViewModels;

public class MoneyViewModel : ViewModelBase
{
    private decimal _money;
    private decimal _expenses;
    private decimal _earnings;

    public decimal Money
    {
        get => _money;
        set => SetProperty(ref _money, value);
    }

    public decimal Expenses
    {
        get => _expenses;
        set => SetProperty(ref _expenses, value);
    }

    public decimal Earnings
    {
        get => _earnings;
        set => SetProperty(ref _earnings, value);
    }

    public MoneyViewModel(decimal startingMoney = 100m)
    {
        Money = startingMoney;
        Expenses = 0m;
        Earnings = 0m;
    }

    public void RecordExpense(decimal amount)
    {
        if (amount <= 0)
            return;

        Expenses += amount;
        Money -= amount;
    }

    public void RecordEarning(decimal amount)
    {
        if (amount <= 0)
            return;

        Earnings += amount;
        Money += amount;
    }
}
