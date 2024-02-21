using System.Collections.Generic;

namespace Service.VisionPro.Services
{
    public class VisionProManager : IVisionProManager
    {
        public Dictionary<string, VisionProInspector> InspectorDic { get; set; } = new Dictionary<string, VisionProInspector>();
        public VisionProManager() { }

        public void Initialize()
        {
            InspectorDic.Add("SWIR", new VisionProInspector());
            InspectorDic.Add("Standard", new VisionProInspector());
        }

        public void Run()
        {
            foreach (var item in InspectorDic.Values)
            {
                item.Start();
            }
        }

        public void Stop()
        {
            foreach (var item in InspectorDic.Values)
            {
                item.Stop();
            }
        }

        public void Dispose()
        {
            foreach (var item in InspectorDic.Values)
            {
                item.Dispose();
            }
        }
    }
}
