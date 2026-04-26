using System.Collections.Generic;

namespace RestaurantSimulator.Models
{
    public class Recipes
    {
        public required string Name { get; set; }
        public required string Difficulty { get; set; }
        public required double SalePrice { get; set; }
        public required List<RequiredIngredients> RequiredIngredients { get; set; }
        public required List<Steps> Steps { get; set; }
    }

    public class RequiredIngredients
    {
        public required string Name { get; set; }
        public required double Quantity { get; set; }
    }

    public class Steps
    {
        public required string Step { get; set; }
        public required int Duration { get; set; }
        public required string StationType { get; set; }
    }
}