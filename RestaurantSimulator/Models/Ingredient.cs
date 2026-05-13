namespace RestaurantSimulator.Models
{
    public class IngredientDefinition
    {
        public required string Name { get; set; }
        public required double InitialStock { get; set; }
        public required string Unit { get; set; }
        public required double Cost { get; set; }
    }

    public class IngredientState
    {
        public required string IngredientName { get; init; }
        public string Status { get; set; } = string.Empty;
    }
}