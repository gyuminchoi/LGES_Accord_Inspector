using Service.Database.Services;
using Service.Postprocessing.Services;
using Service.Setting.Services;
using System;

namespace Service.Save.Services
{
    public interface ISaveManager : IDisposable
    {
        void Initialize(ISettingManager settingManager, IPostprocessingManager ppManager, ISQLiteManager sqliteManager);
        void Start();
        void Stop();
    }
}
