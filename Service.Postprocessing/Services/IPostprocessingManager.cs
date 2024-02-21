using System;
using System.Collections.Generic;

namespace Service.Postprocessing.Services
{
    public interface IPostprocessingManager : IDisposable
    {
        Dictionary<string, Postprocessor> ProcessorDic { get; set; }
        void Start();
        void Stop();
        void Initialize();
    }
}
