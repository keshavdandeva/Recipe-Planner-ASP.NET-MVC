using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RecipePlanner.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Text.RegularExpressions;

namespace RecipePlanner.Controllers{
    public class HomeController : Controller{
        private readonly ILogger<HomeController> _logger;

        public RecipeListModel RecipeListNew = new RecipeListModel();

        public HomeController(ILogger<HomeController> logger){
            _logger = logger;
        }

        public IActionResult Index(){
            importJSON();
            return View(RecipeListNew);
        }

        public IActionResult AddNewRecipe(){
            var recipe = new RecipeModel() { RecipeName = "", RecipeDescription = "", IngredientNames = new List<string>(), IngredientQuantities = new List<float>(), IngredientUnits = new List<string>() };

            return View("AddNewRecipe", recipe);
        }

        public IActionResult EditRecipe(string name_pass){
            importJSON();
            var recipes = RecipeListNew.RecipeList;
            var selected = new RecipeModel();
            foreach (var recipe in recipes){
                if (recipe.RecipeName == name_pass){
                    selected = recipe;
                    break;
                }
            }

            return View("EditRecipe", selected);
        }

        public IActionResult SaveRecipe(string name_pass, string ingredients_pass, string content_pass){
             
            if (name_pass == null || ingredients_pass == null || content_pass == null){
                return RedirectToAction("Index");
            }

            name_pass = name_pass.Trim();
            ingredients_pass = ingredients_pass.Trim();
            content_pass = content_pass.Trim();
            importJSON();
            var recipes = RecipeListNew.RecipeList;

            foreach (var recipe in recipes){
                if (recipe.RecipeName == name_pass){
                    return RedirectToAction("Error");
                }
            }

            var ig_name = new List<string>();
            var ig_quantity = new List<float>();
            var ig_unit = new List<string>();
            
            Regex regex = new Regex(@"(?<=\s)[0-9]+(\.[0-9]+)?(?=\s)");

            foreach (string line in ingredients_pass.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None)){
                if (line == "") continue;

                var matches = regex.Matches(line);
                if (matches.Count != 1){
                    return RedirectToAction("Index");
                }

                var splitted = line.Split(matches[0].Value);
                
                ig_name.Add(splitted[0].Trim());
                ig_quantity.Add(float.Parse(matches[0].Value));
                ig_unit.Add(splitted[1].Trim());
            }

            recipes.Add(new RecipeModel() { RecipeName = name_pass, IngredientNames = ig_name, IngredientQuantities = ig_quantity, IngredientUnits = ig_unit, RecipeDescription = content_pass });
            RecipeListNew.RecipeList = recipes;
            exportJSON();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult SaveEdit(string name_pass, string ingredients_pass,
         string content_pass, string old_name_pass){

            importJSON();
            var recipes = RecipeListNew.RecipeList;
            if (old_name_pass != name_pass){
                
                foreach (var recipe in recipes){
                    if (recipe.RecipeName == name_pass){
                        return RedirectToAction("Error");
                    }
                }
            }

            for (int i = 0; i < RecipeListNew.RecipeList.Count(); i++){
                if (RecipeListNew.RecipeList.ElementAt(i).RecipeName == old_name_pass){
                    RecipeListNew.RecipeList.RemoveAt(i);
                    break;
                }
            }

            if (name_pass == null || ingredients_pass == null || content_pass == null){
                return RedirectToAction("Index");
            }
            name_pass = name_pass.Trim();
            ingredients_pass = ingredients_pass.Trim();
            content_pass = content_pass.Trim();
            
            foreach (var recipe in recipes){
                if (recipe.RecipeName == name_pass){
                     return RedirectToAction("Error");
                }
            }

            var ig_name = new List<string>();
            var ig_quantity = new List<float>();
            var ig_unit = new List<string>();
           
            Regex regex = new Regex(@"(?<=\s)[0-9]+(\.[0-9]+)?(?=\s)"); 

            foreach (string line in ingredients_pass.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None)){
                if (line == "") continue;

                var matches = regex.Matches(line);
                if (matches.Count != 1){
                    return RedirectToAction("Index");
                }

                var splitted = line.Split(matches[0].Value);

                ig_name.Add(splitted[0].Trim());
                ig_quantity.Add(float.Parse(matches[0].Value));
                ig_unit.Add(splitted[1].Trim());
            }

            recipes.Add(new RecipeModel() { RecipeName = name_pass, IngredientNames = ig_name, IngredientQuantities = ig_quantity, IngredientUnits = ig_unit, RecipeDescription = content_pass });
            RecipeListNew.RecipeList = recipes;
            exportJSON();

            return RedirectToAction("Index");
        }

        public IActionResult DeleteRecipe(string name_pass){
            importJSON();
            for (int i = 0; i < RecipeListNew.RecipeList.Count(); i++)
            {
                if (RecipeListNew.RecipeList.ElementAt(i).RecipeName == name_pass)
                {
                    RecipeListNew.RecipeList.RemoveAt(i);
                    break;
                }
            }
            exportJSON();
            return RedirectToAction("Index");
        }

        public void importJSON(){
            StreamReader file = new StreamReader("Recipes.json");
            string data = file.ReadToEnd();
            file.Close();

            JObject all_recipes = JObject.Parse(data);
            foreach (JProperty one_recipe in all_recipes.Properties()){
                RecipeModel recipe = new RecipeModel();
                recipe.RecipeName = one_recipe.Name;
                RecipeListNew.RecipeList.Add(recipe);

                JObject recipe_data = JObject.Parse(one_recipe.Value.ToString());
                foreach (JProperty element in recipe_data.Properties()){
                    if (element.Name == "recipe"){
                        var description = (JArray)element.Value;
                        foreach (var line in description){
                            recipe.RecipeDescription = recipe.RecipeDescription + line.ToString();
                        }
                    }
                    else{
                        recipe.IngredientNames.Add(element.Name.ToString());
                        var array = element.Value.ToString().Split(' ');
                        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                        ci.NumberFormat.CurrencyDecimalSeparator = ".";
                        recipe.IngredientQuantities.Add(float.Parse(array[0].ToString(), NumberStyles.Any, ci));
                        recipe.IngredientUnits.Add(array[1].ToString());
                    }
                }
            }
        }

        public void exportJSON(){
            JObject all_recipes = new JObject();
            foreach (RecipeModel recipe in RecipeListNew.RecipeList){
                JObject new_object = new JObject();
                string temp = recipe.RecipeDescription;
                var split = temp.Split("\n");
                JArray new_array = new JArray();
                new_array = JArray.FromObject(split);
                new_object.Add(new JProperty("recipe", new_array));
                if (recipe.IngredientNames.Count() != 0){
                    for (int i = 0; i < recipe.IngredientNames.Count(); i++){
                        string quantity_unit;
                        quantity_unit = recipe.IngredientQuantities.ElementAt(i).ToString();
                        quantity_unit = quantity_unit + " ";
                        quantity_unit = quantity_unit + recipe.IngredientUnits.ElementAt(i);
                        new_object.Add(new JProperty(recipe.IngredientNames.ElementAt(i), quantity_unit));
                    }
                }
                all_recipes.Add(new JProperty(recipe.RecipeName, new_object));
            }
            System.IO.File.WriteAllText("Recipes.json", JsonConvert.SerializeObject(all_recipes));
        }

        [HttpPost]
        public IActionResult SaveShoppingList(string content_pass){
            
            if (content_pass == null){
                return RedirectToAction("Index");
            }

            content_pass = content_pass.Trim();
            if (content_pass == ""){
                return RedirectToAction("Index");
            }

            importJSON();
            var recipes = RecipeListNew.RecipeList;
            List<Ingredient> shopping_list = new List<Ingredient>();

            foreach (var selected_recipe in content_pass.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None)){
                
                foreach (var recipe in recipes){
                    
                    if (recipe.RecipeName != selected_recipe){
                        continue;
                    }

                    if (recipe.IngredientNames.Count() != 0){
                        
                        for (int i = 0; i < recipe.IngredientNames.Count(); i++){
                            bool found = false;
                            foreach (var recipe_found in shopping_list){
                                
                                if (recipe_found.name == recipe.IngredientNames.ElementAt(i) &&
                                    recipe_found.unit == recipe.IngredientUnits.ElementAt(i)){
                                    
                                    shopping_list.Remove(recipe_found);
                                    shopping_list.Add(new Ingredient(){
                                        name = recipe.IngredientNames.ElementAt(i),
                                        amount = recipe.IngredientQuantities.ElementAt(i) + recipe_found.amount,
                                        unit = recipe.IngredientUnits.ElementAt(i)
                                    });

                                    found = true;
                                    break;
                                }
                            }
                        
                            if (found == false){
                            shopping_list.Add(new Ingredient() { name = recipe.IngredientNames.ElementAt(i), amount = recipe.IngredientQuantities.ElementAt(i), unit = recipe.IngredientUnits.ElementAt(i) });
                            }
                        }

                    }

                    break;
                }
            }

            return View("IngredientList", shopping_list);
        }
        public IActionResult ShoppingList(){
            importJSON();
            return View(RecipeListNew);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(){
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
