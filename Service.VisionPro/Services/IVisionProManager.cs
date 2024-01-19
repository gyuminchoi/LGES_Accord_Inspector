using Service.Database.Services;
using Service.Setting.Models;
using Services.ImageMerge.Services;
using System;
using System.Collections.Generic;

namespace Service.VisionPro.Services
{
    public interface IVisionProManager : IDisposable
    {
        Dictionary<string, VisionProInspector> InspectorDic { get; set; }
        void Initialize(IImageMergeManager ppManager, ISQLiteManager sqliteManager);

        void RecipeLoad(VisionProRecipe recipe);

        void Run();

        void Stop();
    }
}
