using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Camera.Models
{
    public interface ICameraManager
    {
        Dictionary<string, ICamera> CameraDic { get; set; }

        void Opens();

        void Closes();

        void AcqStarts();

        void AcqStops();

        void GrabStarts();
        
        void GrabStops();

        void SetParameters(ECameraGetSetType camGetSetType, string command, object value);

        List<bool> OpenChecks();
        //void TestEnqueue();
    }
}
