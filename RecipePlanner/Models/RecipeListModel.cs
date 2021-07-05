using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipePlanner.Models{
    public class RecipeListModel{

        public List<RecipeModel> RecipeList { get; set; }

        public RecipeListModel(){

            this.RecipeList = new List<RecipeModel>();
        }
    }
}
