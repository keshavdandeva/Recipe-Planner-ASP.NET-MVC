using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipePlanner.Models{
    public class RecipeModel{
        public string RecipeName { get; set; }
        public string RecipeDescription { get; set; }
        public List<string> IngredientNames { get; set; }
        public List<float> IngredientQuantities { get; set; }
        public List<string> IngredientUnits { get; set; }

        public RecipeModel(){

            this.IngredientNames = new List<string>();
            this.IngredientQuantities = new List<float>();
            this.IngredientUnits = new List<string>();
        }
    }
}
