using Service.Camera.Models;
using Service.DeepLearning.Services;
using Service.Postprocessing.Services;
using System;

namespace Service.MainInspection.Services
{
    public interface IMainInpectionManager : IDisposable
    {
        bool IsRun { get; set; }

        void Initialize(ICameraManager cm, IVPDLManager vpdlm, IPostprocessingManager ppm);

        void Run();

        void Stop();


    }
}
