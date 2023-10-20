using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EggCalculator.model
{
    public class RootObject
    {
    public required Dictionary<string, Dictionary<string, Artifact>> Artifacts { get; set; }
        
    }
    public class Artifact
    {
        public string? family { get; set; }
        public int tier { get; set; }
        public Dictionary<string, Ingredient>? Ingredients { get; set; }
        
    }

    public class Ingredient
    {
        public int tier { get; set; }
        public int IngredientCount { get; set; }
    }
    
}