using Prism.Commands;
using Prism.Mvvm;
using Service.IO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dialog.IOMonitor.ViewModels
{
    public class IOControllerViewModel : BindableBase
    {
        private IOManager _ioManager = IOManager.Instance;
        private bool _isMonitoring;
        public IOManager IOManager { get => _ioManager; set => SetProperty(ref _ioManager, value); }
        public bool IsMonitoring { get => _isMonitoring; set => SetProperty(ref _isMonitoring, value); }
        public DelegateCommand ClosingCommand => new DelegateCommand(OnClosing);

        public IOControllerViewModel() { }

        private void OnClosing()
        {
            IsMonitoring = false;
        }
    }
}
