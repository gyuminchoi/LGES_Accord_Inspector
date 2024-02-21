using Service.DeepLearning.Models;
using Service.Setting.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViDi2;

namespace Service.DeepLearning.Services
{
    public interface IVPDLManager : IDisposable
    {
        void Initialize(ISettingManager sm);
        bool GetRedToolResult(ISample sample, string toolName);
        VPDLResult VPDLInspect(Bitmap bmp);
    }
}
