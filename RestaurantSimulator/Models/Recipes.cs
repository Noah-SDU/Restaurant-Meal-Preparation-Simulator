using System.Collections.Generic;

namespace RestaurantSimulator.Models
{
    public class Recipe 
    {
        public required string Name { get; set; }
        public required string Difficulty { get; set; }
        public required double SalePrice { get; set; }
        public required IReadOnlyList<RequiredIngredient> RequiredIngredients { get; set; }
        public required IReadOnlyList<Step> Steps { get; set; }
    }

    public class RequiredIngredient
    {
        public required string Name { get; set; }
        public required double Quantity { get; set; }
        public string Unit { get; set; } = "";
    }

    public class Step
    {
        public required string Name { get; set; }
        public required int Duration { get; set; }
        public required string StationType { get; set; }
    }
}