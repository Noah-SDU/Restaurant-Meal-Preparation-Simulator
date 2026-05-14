using CommunityToolkit.Mvvm.ComponentModel;

namespace RestaurantSimulator.Services;

public interface IGameSettings
{
    double TimeMultiplier { get; set; }           // global multiplier
    double EasyMultiplier { get; set; }
    double MediumMultiplier { get; set; }
    double HardMultiplier { get; set; }

    double GetDifficultyMultiplier(string difficulty);
}

public class GameSettings : ObservableObject, IGameSettings
{
    private double _timeMultiplier = 1.0;
    private double _easy = 1.0;
    private double _medium = 1.25;
    private double _hard = 1.5;

    public double TimeMultiplier
    {
        get => _timeMultiplier;
        set => SetProperty(ref _timeMultiplier, value);
    }

    public double EasyMultiplier
    {
        get => _easy;
        set => SetProperty(ref _easy, value);
    }

    public double MediumMultiplier
    {
        get => _medium;
        set => SetProperty(ref _medium, value);
    }

    public double HardMultiplier
    {
        get => _hard;
        set => SetProperty(ref _hard, value);
    }

    public double GetDifficultyMultiplier(string difficulty)
        => difficulty?.Trim().ToLowerInvariant() switch
        {
            "easy" => EasyMultiplier,
            "medium" => MediumMultiplier,
            "hard" => HardMultiplier,
            _ => 1.0
        };
}
