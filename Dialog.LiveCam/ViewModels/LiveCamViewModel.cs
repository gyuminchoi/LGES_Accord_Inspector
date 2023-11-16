using Dialog.LiveCam.Models;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Service.Camera.Models;
using Service.Logger.Services;
using Service.Setting.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;

namespace Dialog.LiveCam.ViewModels
{
    public class LiveCamViewModel : BindableBase, IDialogAware, IDisposable
    {
        private LogWrite _logWrite = LogWrite.Instance;
        private ICameraManager _camManager;
        private ISettingManager _settingManager;
        
        public string Title => "Live Cam";
        public event Action<IDialogResult> RequestClose;
        public ObservableCollection<LiveCamera> LiveCams { get; set; } = new ObservableCollection<LiveCamera>();

        public LiveCamViewModel(ICameraManager cm, ISettingManager sm)
        {
            _camManager = cm;
            _settingManager = sm;
        }

        public bool CanCloseDialog() 
        {
            var isSave = MessageBox.Show("Do you want to save it?", "Save", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if(isSave == MessageBoxResult.Cancel)
                return false;

            foreach (var cam in LiveCams)
            {
                if(isSave == MessageBoxResult.Yes)
                    cam.SaveParameter();
                else if(isSave == MessageBoxResult.No)
                    cam.RollbackParameter();
            }

            return true;
        }

        public void OnDialogClosed() => Dispose();

        public void OnDialogOpened(IDialogParameters parameters)
        {
            try
            {
                if (!CheckCameraConnection(_camManager))
                {
                    _logWrite?.Info("Connected camera does not exist.", true);
                    return;
                }
                InitializeLiveCam();

                _camManager.AcqStarts();
                _camManager.GrabStarts();

                foreach (var liveCam in LiveCams)
                  {
                    liveCam.Camera.ContinueSWTrigExecute();
                }

                _logWrite.Info("Start Live Cam Trig Thread");
            }
            catch (Exception err)
            {
                _logWrite?.Error(err);
            }
        }

        private bool CheckCameraConnection(ICameraManager camManager)
        {
            int camCount = camManager.CameraDic.Count;

            if (camCount == 0)
                return false;
            else
                return true;
        }

        private void InitializeLiveCam()
        {
            try
            {
                AddCams();
                SetGirdDefinitions();
                _logWrite?.Info($"Connected camera count : {LiveCams.Count}");
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
                var camConfig = new LiveCamera(item.Value, _settingManager.AppSetting.GeneralSetting);

                if (LiveCams.Count == 0)
                {
                    LiveCams.Add(camConfig);
                    continue;
                }

                int camNum = int.Parse(LiveCams[0].Camera.CamConfig.UserID.Replace("Cam", ""));
                int currentCamNum = int.Parse(camConfig.Camera.CamConfig.UserID.Replace("Cam", ""));

                if (currentCamNum < camNum)
                    LiveCams.Insert(0, camConfig);
                else
                    LiveCams.Add(camConfig);
            }
        }

        private void SetGirdDefinitions()
        {
            // Max Index Row
            int rowMax = LiveCams.Count > 2 ? 2 : 1;

            for (int i = 0; i < rowMax; i++)
            {
                // Max Index Column
                int colMax = 0;
                if (i == 0)
                    colMax = LiveCams.Count >= 2 ? 2 : 1;

                if (i == 1)
                    colMax = LiveCams.Count - 2 >= 2 ? 2 : 1;

                for (int j = 0; j < colMax; j++)
                {
                    if (i == 0)
                    {
                        LiveCams[j].Col = j;
                        LiveCams[j].Row = 0;
                    }

                    if (i == 1)
                    {
                        LiveCams[j + 2].Col = j;
                        LiveCams[j + 2].Row = 1;
                    }
                }
            }
        }

        public void Dispose()
        {
            foreach (var liveCam in LiveCams)
            {
                liveCam.Camera.StopContinueTrigExecute();
            }

            _camManager.GrabStops();
            _camManager.AcqStops();

            foreach (var liveCam in LiveCams)
            {
                liveCam.Dispose();
            }
            LiveCams.Clear();
        }
    }
}
