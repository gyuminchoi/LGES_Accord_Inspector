using Service.Camera.Models;
using Service.ConnectionCheck.Models;
using Service.Database.Services;
using Service.VisionPro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.ConnectionCheck.Services
{
    public interface IConnectionCheckManager : IDisposable
    {
        void Initialize(ICameraManager cm, IVisionProManager vpm, ISQLiteManager sqliteManager);
        ServiceState ServiceStates { get; set; }
        void Start();
        void Stop();

    }
}
