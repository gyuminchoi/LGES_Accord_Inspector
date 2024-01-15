using Service.VisionPro.Models;
using Service.VisionPro.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Poseprocessing.Services
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
            ProcessorDic.Add("Top", new Postprocessor(_vpManager.InspectorDic["Top"].ResultQueue));
            ProcessorDic.Add("Bottom", new Postprocessor(_vpManager.InspectorDic["Bottom"].ResultQueue));
        }

        public void Start()
        {
            
        }

        public void Stop()
        {
            
        }
    }
}
