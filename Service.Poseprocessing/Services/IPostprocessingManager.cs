using Service.VisionPro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Poseprocessing.Services
{
    public interface IPostprocessingManager
    {
        Dictionary<string, Postprocessor> ProcessorDic { get; set; }
        void Start();
        void Stop();
        void Initialize(IVisionProManager vpm);
    }
}
