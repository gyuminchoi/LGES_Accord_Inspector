using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Setting.Models
{
    public class IOSetting : BindableBase
    {
        private string _ipAddress;

        public string IPAddress { get => _ipAddress; set => SetProperty(ref _ipAddress, value); }   

        public IOSetting(string ipAddr) 
        {
            IPAddress = ipAddr;
        }
    }
}
