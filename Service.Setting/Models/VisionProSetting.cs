using Prism.Mvvm;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Setting.Models
{
    public class VisionProSetting : BindableBase
    {
        private Dictionary<string, VisionProRecipe> _recipes;
         
        public Dictionary<string, VisionProRecipe> Recipes { get => _recipes; set => SetProperty(ref _recipes, value); }  
        
        public VisionProSetting(Dictionary<string, VisionProRecipe> recipes) 
        {
            Recipes = recipes;
        }
    }
}
