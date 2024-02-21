using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Setting.Models
{
    public class VPDLSetting : BindableBase
    {
        private string _workspacePath;
        private string _workspaceName;
        private string _streamName;
        private string _toolName;

        public string WorkspacePath { get => _workspacePath; set => SetProperty(ref _workspacePath, value); }
        public string Workspacename { get => _workspaceName; set => SetProperty(ref _workspaceName, value); }
        public string StreamName { get => _streamName; set => SetProperty(ref _streamName,value); }
        public string ToolName { get => _toolName; set => SetProperty(ref _toolName, value); }
    }
}
