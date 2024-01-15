using Service.Save.Models;
using Service.Setting.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Save.Services
{
    public interface ISaveManager : IDisposable
    {
        ConcurrentQueue<ImageData> ImageSaveQueue { get; set; }
        void Initialize(ISettingManager settingManager);
        void Start();
        void Stop();
    }
}
