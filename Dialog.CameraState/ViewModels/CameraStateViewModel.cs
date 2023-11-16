using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Service.Camera.Models;
using Service.Logger.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;

namespace Dialog.CameraState.ViewModels
{
    public class CameraStateViewModel : BindableBase, IDialogAware
    {
        private ICameraManager _camManager;
        private LogWrite _logWrite = LogWrite.Instance;

        public string Title => "Camera State";

        public event Action<IDialogResult> RequestClose;

        public ObservableCollection<ICamera> Cams { get; set; } = new ObservableCollection<ICamera>();
        
        public CameraStateViewModel(ICameraManager cm)
        {
            _camManager = cm;
        }

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            try
            {
                InitializeCameraState();
            }
            catch (Exception err)
            {
                _logWrite?.Error(err);
            }
        }

        private void InitializeCameraState()
        {
            try
            {
                AddCams();

                _logWrite?.Info("Complete camera state dialog init");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void AddCams()
        {
            foreach (var item in _camManager.CameraDic)
            {
                if (Cams.Count == 0)
                {
                    Cams.Add(item.Value);
                    continue;
                }

                int camNum = int.Parse(Cams[0].CamConfig.UserID.Replace("Cam", ""));
                int currentCamNum = int.Parse(item.Value.CamConfig.UserID.Replace("Cam", ""));

                if (currentCamNum < camNum)
                    Cams.Insert(0, item.Value);
                else
                    Cams.Add(item.Value);
            }
        }
    }
}
