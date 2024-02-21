﻿using System;
using System.Collections.Generic;

namespace Service.VisionPro.Services
{
    public interface IVisionProManager : IDisposable
    {
        Dictionary<string, VisionProInspector> InspectorDic { get; set; }
        void Initialize();

        void Run();

        void Stop();
    }
}
