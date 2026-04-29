namespace RestaurantSimulator.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
	private ViewModelBase? _currentViewModel;

	public ViewModelBase? CurrentViewModel
	{
		get => _currentViewModel;
		set => SetProperty(ref _currentViewModel, value);
	}
}
