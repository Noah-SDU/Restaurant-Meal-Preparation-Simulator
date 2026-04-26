using System.Collections.Generic;

namespace RestaurantSimulator.Models
{
    public class RestaurantData
    {
        public required List<Stations> Stations { get; set; }
        public required List<Ingredients> Ingredients { get; set; }
        public required List<Recipes> Recipes { get; set; }
    }
}