using Service.Camera.Models;
using Service.Postprocessing.Services;
using Service.VisionPro.Services;
using System;

namespace Service.MainInspection.Services
{
    public interface IMainInpectionManager : IDisposable
    {
        bool IsRun { get; set; }

        void Initialize(ICameraManager cm, IVisionProManager vpm, IPostprocessingManager ppm);

        void Run();

        void Stop();


    }
}
