using BarcodeLabel.Core.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Service.Camera.Models;
using Service.ImageMerge.Models;
using Service.Postprocessing.Services;
using Service.Save.Services;
using Service.Setting.Models;
using Service.Setting.Services;
using Service.VisionPro.Services;
using Services.ImageMerge.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using UI.Controller.Models;

namespace UI.Controller.ViewModels
{
    public class ControllerViewModel : BindableBase, IDisposable
    {
        private ICameraManager _camManager;
        private IEventAggregator _eventAggregator;
        private ISettingManager _settingManager;
        private IVisionProManager _visionProManager;
        private IImageMergeManager _imageMergeManager;
        private IPostprocessingManager _postProcessingManager;
        private ISaveManager _saveManager;
        private EInspectionState _inspectionState = EInspectionState.Stopped;
        private InspectionStatusChangeEvent _inspectionStatusChangeEvent;
        private VisionProSetting _visionProSetting;
        private VisionProRecipe _recipe;
        private ServicesInitCompleteEvent _servicesInitCompleteEvent;
        private bool _isEnabledGrabBtn = false;
        private bool _isEnabledRecipe = true;
        //TODO :Test
        private Thread _testThread = new Thread(() => { });
        public EInspectionState InspectionState { get => _inspectionState; set => SetProperty(ref _inspectionState, value); }
        public ObservableCollection<KeyValuePair<string, VisionProRecipe>> VisionProRecipe { get; set; } = new ObservableCollection<KeyValuePair<string, VisionProRecipe>>();
        public VisionProRecipe Recipe { get => _recipe; set => SetProperty(ref _recipe, value); }
        public bool IsEnabledGrabBtn { get => _isEnabledGrabBtn; set => SetProperty(ref _isEnabledGrabBtn, value); }
        public bool IsEnabledRecipe { get => _isEnabledRecipe; set => SetProperty(ref _isEnabledRecipe, value); } 

        public DelegateCommand<object> BtnStartStopCommand => new DelegateCommand<object>(OnInspectionStartStop);

        //TODO :Test 
        public DelegateCommand BtnTestCommand => new DelegateCommand(OnTest);

 
        ~ControllerViewModel() { Dispose(); } 

        private void OnTest()
        {
            _testThread = new Thread(() => 
            {
                while (true)
                {
                    Bitmap bmp1 = new Bitmap(@"C:\Users\TSgyuminChoi\Desktop\대원제약 검토 자료\1101\Test2\Bot Result\1.bmp");
                    _imageMergeManager.TopMergeBitmapQueue.Enqueue(new MergeBitmap() { Bmp = bmp1.Clone() as Bitmap});
                    _imageMergeManager.BotMergeBitmapQueue.Enqueue(new MergeBitmap() { Bmp = bmp1.Clone() as Bitmap });
                    bmp1.Dispose();
                    Thread.Sleep(300);
                    Bitmap bmp2 = new Bitmap(@"C:\Users\TSgyuminChoi\Desktop\대원제약 검토 자료\1101\Test2\Bot Result\12.bmp");
                    _imageMergeManager.TopMergeBitmapQueue.Enqueue(new MergeBitmap() { Bmp = bmp2.Clone() as Bitmap });
                    _imageMergeManager.BotMergeBitmapQueue.Enqueue(new MergeBitmap() { Bmp = bmp2.Clone() as Bitmap });
                    bmp2.Dispose();
                    Thread.Sleep(300);
                    Bitmap bmp3 = new Bitmap(@"C:\Users\TSgyuminChoi\Desktop\대원제약 검토 자료\1101\Test2\Bot Result\2.bmp");
                    _imageMergeManager.TopMergeBitmapQueue.Enqueue(new MergeBitmap() { Bmp = bmp3.Clone() as Bitmap });
                    _imageMergeManager.BotMergeBitmapQueue.Enqueue(new MergeBitmap() { Bmp = bmp3.Clone() as Bitmap });
                    bmp3.Dispose();
                    Thread.Sleep(300);
                    Bitmap bmp4 = new Bitmap(@"C:\Users\TSgyuminChoi\Desktop\대원제약 검토 자료\1101\Test2\Bot Result\3.bmp");
                    _imageMergeManager.TopMergeBitmapQueue.Enqueue(new MergeBitmap() { Bmp = bmp4.Clone() as Bitmap });
                    _imageMergeManager.BotMergeBitmapQueue.Enqueue(new MergeBitmap() { Bmp = bmp4.Clone() as Bitmap });
                    bmp4.Dispose();
                    Thread.Sleep(300);
                }
            });
            _testThread.Start();
        }

        public ControllerViewModel(ICameraManager cm, IEventAggregator ea, ISettingManager sm, IVisionProManager vm, IImageMergeManager imm, IPostprocessingManager ppm, ISaveManager saveManager)
        {
            _camManager = cm;
            _eventAggregator = ea;
            _settingManager = sm;
            _visionProManager = vm;
            _imageMergeManager = imm;
            _postProcessingManager = ppm;
            _saveManager = saveManager;

            _inspectionStatusChangeEvent = _eventAggregator.GetEvent<InspectionStatusChangeEvent>();
            _inspectionStatusChangeEvent.Subscribe(OnChangeIsEnable);

            _servicesInitCompleteEvent = _eventAggregator.GetEvent<ServicesInitCompleteEvent>();
            _servicesInitCompleteEvent.Subscribe(OnInitializeServices);
        }

        private void OnInitializeServices()
        {
            VisionProRecipe.Clear();
            foreach (var recipe in _settingManager.AppSetting.VisionProSetting.Recipes)
            {
                VisionProRecipe.Add(recipe);
            }

            Recipe = VisionProRecipe.First().Value;
        }

        private void OnChangeIsEnable(bool inspectionStatus)
        {
            if (inspectionStatus)
            {
                IsEnabledGrabBtn = true;
                IsEnabledRecipe = false;
            }
            else
            {
                IsEnabledGrabBtn = false;
                IsEnabledRecipe = true;
            }
        }

        private void OnInspectionStartStop(object obj)
        {
            EInspectionState state = (EInspectionState)obj;
            switch (state) 
            {
                case EInspectionState.Stopped:
                    _eventAggregator.GetEvent<InspectionStatusChangeEvent>().Publish(true);

                    _saveManager.Start();

                    _postProcessingManager.Load(Recipe);
                    _postProcessingManager.Start();

                    _visionProManager.RecipeLoad(Recipe);
                    _visionProManager.Run();

                    _imageMergeManager.Start();

                    _camManager.AcqStarts();
                    _camManager.GrabStarts();

                    foreach (var item in _camManager.CameraDic.Values)
                    {
                        item.ContinueSWTrigExecute();
                    }

                    InspectionState = EInspectionState.Running;
                    return;

                case EInspectionState.Running:
                    _eventAggregator.GetEvent<InspectionStatusChangeEvent>().Publish(false);

                    foreach (var item in _camManager.CameraDic.Values)
                    {
                        item.StopContinueTrigExecute();
                    }

                    _camManager.GrabStarts();
                    _camManager.AcqStarts();

                    _imageMergeManager.Stop();

                    _visionProManager.Stop();

                    _postProcessingManager.Stop();

                    _saveManager.Stop();

                    //_testThread.Abort();

                    InspectionState = EInspectionState.Stopped;
                    return;
            }
            

            //_eventAggregator.GetEvent<StartStopCompleteEvent>().Publish("Start");
            //Thread cam1ExecuteThread = new Thread(() => 
            //{
            //    while (true) 
            //    {
            //        _camManager.CameraDic["Cam1"].SWTriggerExecute(_camManager.CameraDic["Cam1"]);
            //        Thread.Sleep(500);
            //    }
            //});
            //cam1ExecuteThread.Start();

            //Thread cam2ExecuteThread = new Thread(() =>
            //{
            //    while (true)
            //    {
            //        _camManager.CameraDic["Cam2"].SWTriggerExecute(_camManager.CameraDic["Cam2"]);
            //        Thread.Sleep(500);
            //    }
            //});
            //cam2ExecuteThread.Start();
        }

        public void Dispose()
        {
            
        }
    }
}
