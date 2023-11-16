using BarcodeLabel.Core.Events;
using Dialog.DirectorySelecton.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.MenuBar.ViewModels
{
    public class MenuBarViewModel : BindableBase
    {
        private IDialogService _dialogService;
        private IEventAggregator _eventAggregator;
        private InspectionStatusChangeEvent _inspectionStatusChangeEvent;
        private bool _isEnabledLiveCam = true;
        private bool _isEnabledSetting = true;

        public bool IsEnabledLiveCam { get => _isEnabledLiveCam; set => SetProperty(ref _isEnabledLiveCam, value); }
        public bool IsEnabledSetting { get => _isEnabledSetting; set => SetProperty(ref _isEnabledSetting, value); }

        public DelegateCommand BtnDirectoryCommmand => new DelegateCommand(OnShowSelectionDirectoryDialog);
        public DelegateCommand BtnSettingCommand => new DelegateCommand(OnShowSettingDialog);
        public DelegateCommand BtnLiveCamCommand => new DelegateCommand(OnShowLiveCamDialog);


        public MenuBarViewModel(IDialogService ds, IEventAggregator ea)
        {
            _dialogService = ds;
            _eventAggregator = ea;

            _inspectionStatusChangeEvent = _eventAggregator.GetEvent<InspectionStatusChangeEvent>();
            _inspectionStatusChangeEvent.Subscribe(OnChangeIsEnable);
        }

        private void OnChangeIsEnable(bool inspectionStatus)
        {
            if(inspectionStatus)
            {
                IsEnabledLiveCam = false;
                IsEnabledSetting = false;
            }
            else
            {
                IsEnabledLiveCam = true;
                IsEnabledSetting = true;
            }
        }

        private void OnShowSelectionDirectoryDialog()
        {
            DirectorySelectionWindow directorySelectionWindow = new DirectorySelectionWindow();
            directorySelectionWindow.Show();
        }

        private void OnShowSettingDialog()
        {
            _dialogService.ShowDialog("AppSettingDialog");
        }

        private void OnShowLiveCamDialog()
        {
            _dialogService.ShowDialog("LiveCamDialog");
        }
    }
}
