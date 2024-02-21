using CREVIS_SWIR_Inspector.Core.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Service.Camera.Models;
using Service.MainInspection.Services;
using Service.Postprocessing.Services;
using Service.VisionPro.Services;
using System;
using UI.Controller.Models;

namespace UI.Controller.ViewModels
{
    public class ControllerViewModel : BindableBase, IDisposable
    {
        private IEventAggregator _eventAggregator;
        private IMainInpectionManager _miManager;
        private EInspectionState _inspectionState = EInspectionState.Stopped;
        private InspectionStatusChangeEvent _inspectionStatusChangeEvent;
        private ServicesInitCompleteEvent _servicesInitCompleteEvent;
        public EInspectionState InspectionState { get => _inspectionState; set => SetProperty(ref _inspectionState, value); }

        public DelegateCommand<object> BtnStartStopCommand => new DelegateCommand<object>(OnInspectionStartStop);

        ~ControllerViewModel() => Dispose();


        public ControllerViewModel(IEventAggregator ea, IMainInpectionManager mim)
        {
            _eventAggregator = ea;
            _miManager = mim;

            _inspectionStatusChangeEvent = _eventAggregator.GetEvent<InspectionStatusChangeEvent>();
            _inspectionStatusChangeEvent.Subscribe(OnChangeIsEnable);

            _servicesInitCompleteEvent = _eventAggregator.GetEvent<ServicesInitCompleteEvent>();
            _servicesInitCompleteEvent.Subscribe(OnInitializeServices);
        }

        private void OnInitializeServices()
        {
        }

        private void OnChangeIsEnable(bool inspectionStatus)
        {
        }

        private void OnInspectionStartStop(object obj)
        {
            EInspectionState state = (EInspectionState)obj;
            switch (state) 
            {
                case EInspectionState.Stopped:
                    _eventAggregator.GetEvent<InspectionStatusChangeEvent>().Publish(true);

                    _miManager.Run();

                    InspectionState = EInspectionState.Running;
                    return;

                case EInspectionState.Running:
                    _eventAggregator.GetEvent<InspectionStatusChangeEvent>().Publish(false);

                    _miManager.Stop();

                    InspectionState = EInspectionState.Stopped;
                    return;
            }
        }

        public void Dispose()
        {
            
        }
    }
}
