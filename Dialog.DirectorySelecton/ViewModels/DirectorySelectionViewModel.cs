using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Service.Logger.Services;
using Service.Setting.Models;
using Service.Setting.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Dialog.DirectorySelecton.ViewModels
{
    public class DirectorySelectionViewModel : BindableBase
    {
        private LogWrite _logWrite = LogWrite.Instance;
        private AppSetting _appSetting;

        public DelegateCommand<Window> DeactivatedCommand => new DelegateCommand<Window>(OnCloseWindow);
        public DelegateCommand BtnLogDirectoryCommand => new DelegateCommand(OnLogDirectoryOpen);
        public DelegateCommand BtnImageDirectoryCommand => new DelegateCommand(OnImageDirectoryOpen);
        public DelegateCommand BtnDataDirectoryCommand => new DelegateCommand(OnDataDirectoryOpen);

        public DirectorySelectionViewModel(ISettingManager sm)
        {
            _appSetting = sm.AppSetting;
        }

        private void OnDataDirectoryOpen()
        {
            try { Process.Start(_appSetting.DataSetting.SavePath); }
            catch (Exception err) { _logWrite?.Error(err, true, true); }
        }

        private void OnImageDirectoryOpen()
        {
            try { Process.Start(_appSetting.ImageSetting.SavePath); }
            catch (Exception err) { _logWrite?.Error(err, true, true); }
        }

        private void OnLogDirectoryOpen()
        {
            try { Process.Start(_appSetting.GeneralSetting.LogSavePath); }
            catch (Exception err) { _logWrite?.Error(err, true, true); }
        }

        private void OnCloseWindow(Window window)
        {
            window.Close();
        }

    }
}
