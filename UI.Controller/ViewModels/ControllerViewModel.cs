using BarcodeLabel.Core.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Service.Camera.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UI.Controller.Models;

namespace UI.Controller.ViewModels
{
    public class ControllerViewModel : BindableBase
    {
        private ICameraManager _camManager;
        private IEventAggregator _eventAggregator;
        private InspectionStatusChangeEvent _inspectionStatusChangeEvent;
        private bool _isEnabledGrabBtn = false;
        private bool _isEnabledRecipe = true;
        private EInspectionState _inspectionState = EInspectionState.Stopped;

        public EInspectionState InspectionState { get => _inspectionState; set =>SetProperty(ref _inspectionState, value); }
        public bool IsEnabledGrabBtn { get => _isEnabledGrabBtn; set => SetProperty(ref _isEnabledGrabBtn, value); }
        public bool IsEnabledRecipe { get => _isEnabledRecipe; set => SetProperty(ref _isEnabledRecipe, value); } 

        public DelegateCommand<object> BtnStartStopCommand => new DelegateCommand<object>(OnInspectionStartStop);

        public ControllerViewModel(ICameraManager cm, IEventAggregator ea)
        {
            _camManager = cm;
            _eventAggregator = ea;

            _inspectionStatusChangeEvent = _eventAggregator.GetEvent<InspectionStatusChangeEvent>();
            _inspectionStatusChangeEvent.Subscribe(OnChangeIsEnable);
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
                    InspectionState = EInspectionState.Running;
                    return;

                case EInspectionState.Running:
                    _eventAggregator.GetEvent<InspectionStatusChangeEvent>().Publish(false);
                    InspectionState = EInspectionState.Stopped;
                    return;
                
            }
            //_camManager.AcqStarts();
            //_camManager.GrabStarts();

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
    }
}
