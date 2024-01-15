using Service.Setting.Models;
using Services.ImageMerge.Services;
using System.Collections.Generic;

namespace Service.VisionPro.Services
{
    public class VisionProManager : IVisionProManager
    {
        
        private IImageMergeManager _imManager;
        public Dictionary<string, VisionProInspector> InspectorDic { get; set; } = new Dictionary<string, VisionProInspector>();
        public VisionProManager() { }

        public void Initialize(IImageMergeManager imManager)
        {
            _imManager = imManager;
            InspectorDic.Add("Top", new VisionProInspector(_imManager.TopMergeBitmapQueue));
            InspectorDic.Add("Bottom", new VisionProInspector(_imManager.BotMergeBitmapQueue));
        }

        public void RecipeLoad(VisionProRecipe recipe)
        {
            foreach (var item in InspectorDic.Values)
            {
                item.RecipeLoad(recipe);
            }
        }

        public void Run()
        {
            foreach (var item in InspectorDic.Values)
            {
                item.Start();
            }
        }

        public void Stop()
        {
            foreach (var item in InspectorDic.Values)
            {
                item.Stop();
            }
        }

        public void Dispose()
        {
            foreach (var item in InspectorDic.Values)
            {
                item.Dispose();
            }
        }
    }
}
