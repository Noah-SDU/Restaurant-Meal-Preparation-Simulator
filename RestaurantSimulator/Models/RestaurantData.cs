using System.Collections.Generic;

namespace RestaurantSimulator.Models
{
    public class RestaurantData
    {
        public required List<Station> Stations { get; set; }
        public required List<IngredientDefinition> Ingredients { get; set; }
        public required List<Recipe> Recipes { get; set; }
    }
}