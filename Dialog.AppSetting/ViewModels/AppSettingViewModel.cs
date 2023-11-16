using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Service.Setting.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Dialog.AppSetting.ViewModels
{
    public class AppSettingViewModel : BindableBase, IDialogAware
    {
        public string Title => "App Setting Dialog";
        private ISettingManager _settingManager;
        public ISettingManager SettingManager { get => _settingManager; set => SetProperty(ref _settingManager, value); }

        public event Action<IDialogResult> RequestClose;

        public DelegateCommand BtnSelectImageSavePathCommand => new DelegateCommand(OnSelectImageSavePath);


        public AppSettingViewModel(ISettingManager sm)
        {
            _settingManager = sm;
        }

        public bool CanCloseDialog()
        {
            var isSave = MessageBox.Show("Do you want to save it?", "Save", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            switch (isSave)
            {
                case MessageBoxResult.Yes:
                    SettingManager.Serialize();
                    return true;

                case MessageBoxResult.No:
                    SettingManager.Deserialize();
                    return true;

                case MessageBoxResult.Cancel:
                default:
                    return false;
            }

        }


        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }

        private void OnSelectImageSavePath()
        {
            string path = GetFolderPath();

            if (path != null)
                SettingManager.AppSetting.ImageSetting.SavePath = path;
        }

        private string GetFolderPath()
        {
            var selectFileDialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            if (selectFileDialog.ShowDialog() == CommonFileDialogResult.Ok) 
                return selectFileDialog.FileName;
            else 
                return null;
        }
    }
}
