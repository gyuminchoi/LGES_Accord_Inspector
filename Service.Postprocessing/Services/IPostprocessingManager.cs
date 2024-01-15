using Service.Setting.Models;
using Service.VisionPro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Postprocessing.Services
{
    public interface IPostprocessingManager : IDisposable
    {
        Dictionary<string, Postprocessor> ProcessorDic { get; set; }
        void Load(VisionProRecipe recipe);
        void Start();
        void Stop();
        void Initialize(IVisionProManager vpm);
    }
}
