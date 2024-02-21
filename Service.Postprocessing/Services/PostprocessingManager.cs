using System.Collections.Generic;

namespace Service.Postprocessing.Services
{
    public class PostprocessingManager : IPostprocessingManager
    {
        public Dictionary<string, Postprocessor> ProcessorDic { get; set; } = new Dictionary<string, Postprocessor>();

        public PostprocessingManager() { }

        public void Initialize()
        {
            ProcessorDic.Add("SWIR", new Postprocessor());
            ProcessorDic.Add("Standard", new Postprocessor());
        }

        public void Start()
        {
            foreach (var processor in ProcessorDic.Values)
            {
                processor.Start();
            }
        }

        public void Stop()
        {
            foreach (var processor in ProcessorDic.Values)
            {
                processor.Stop();
            }
        }

        public void Dispose()
        {
            foreach(var processor in ProcessorDic.Values)
            {
                processor.Dispose();
            }
        }
    }
}
