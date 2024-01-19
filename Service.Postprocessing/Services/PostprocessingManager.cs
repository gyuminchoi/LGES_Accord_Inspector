using Service.Setting.Models;
using Service.VisionPro.Models;
using Service.VisionPro.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Postprocessing.Services
{
    public class PostprocessingManager : IPostprocessingManager
    {
        private IVisionProManager _vpManager;
        public Dictionary<string, Postprocessor> ProcessorDic { get; set; } = new Dictionary<string, Postprocessor>();

        public PostprocessingManager()
        {
        }

        public void Initialize(IVisionProManager vpm)
        {
            _vpManager = vpm;

            ProcessorDic.Add("Top", new Postprocessor(_vpManager.InspectorDic["Top"].VisionProResultQueue));
            ProcessorDic.Add("Bottom", new Postprocessor(_vpManager.InspectorDic["Bottom"].VisionProResultQueue));
        }

        public void Load(VisionProRecipe recipe)
        {
            foreach (var processor in ProcessorDic.Values) 
            {
                processor.RecipeLoad(recipe);
            }
        }

        public void Start()
        {
            foreach (var processor in ProcessorDic.Values)
            {
                processor.Start();
            }
        }

        public void Stop()
        {
            foreach (var processor in ProcessorDic.Values)
            {
                processor.Stop();
            }
        }

        public void Dispose()
        {
            foreach(var processor in ProcessorDic.Values)
            {
                processor.Dispose();
            }
        }

        
    }
}
