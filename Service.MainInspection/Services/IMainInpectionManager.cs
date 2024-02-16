using Service.Camera.Models;
using Service.Postprocessing.Services;
using Service.Save.Services;
using Service.Setting.Models;
using Service.VisionPro.Services;
using Services.ImageMerge.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.MainInspection.Services
{
    public interface IMainInpectionManager : IDisposable
    {
        bool IsRun { get; set; }

        void Initialize(ICameraManager cm, IImageMergeManager imm, IVisionProManager vpm, IPostprocessingManager ppm, ISaveManager sm);

        void Run(VisionProRecipe recipe);

        void Stop();


    }
}
