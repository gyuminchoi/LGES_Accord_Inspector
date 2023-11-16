using Service.IO.Models;
using Service.IO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.IOController.Models
{
    public class IOManBitDataCommandParameter
    {
        public IOManager IOManagerParameter { get; set; }
        public BitData BitDataParameter { get; set; }
    }
}
