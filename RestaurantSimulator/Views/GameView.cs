using Avalonia.Controls;
using Avalonia.Input;

namespace RestaurantSimulator.Views;

public partial class GameView : UserControl
{
    public GameView()
    {
        InitializeComponent();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Return && DataContext is ViewModels.GameViewModel vm)
        {
            vm.ExecuteCommandCommand.Execute(null);
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }
}
