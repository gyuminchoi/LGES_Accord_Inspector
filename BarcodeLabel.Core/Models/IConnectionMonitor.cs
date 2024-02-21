using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CREVIS_SWIR_Inspector.Core.Models
{
    public interface IConnectionMonitor
    {
        string Name { get; set; }
        bool IsConnected { get; set; }
    }
}
