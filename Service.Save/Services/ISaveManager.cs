using Service.Database.Services;
using Service.Postprocessing.Services;
using Service.Setting.Services;
using Service.VisionPro.Models;
using Service.VisionPro.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Service.Save.Services
{
    public interface ISaveManager : IDisposable
    {
        void Initialize(ISettingManager sm, IVisionProManager vpm, ISQLiteManager sqliteManager);
        void Start();
        void Stop();
        void DataSave(VisionProResult result);
        ConcurrentQueue<VisionProResult> SaveQueue { get; set; }
        bool IsRun { get; set; }
    }
}
