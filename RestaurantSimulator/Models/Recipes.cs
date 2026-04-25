using System.Collections.Generic;

namespace RestaurontSimulator.Models
{
    public class Recipes
    {
        public required string Name { get; set; }
        public required string Difficulty { get; set; }
        public required double Price { get; set; }
        public required List<Ingredients> Ingredients { get; set; }
    }
}