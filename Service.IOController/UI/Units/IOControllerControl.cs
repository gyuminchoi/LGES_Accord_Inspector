using BarcodeLabel.Core.Services;
using Service.CustomException.Services.ErrorService.HandledExceptions;
using Service.IO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Service.IOController.UI.Units
{
    public class IOControllerControl : Control
    {
        public static readonly DependencyProperty IsMonitoringProperty =
            DependencyProperty.Register(
                nameof(IsMonitoring),                                   // 프로퍼티 이름 설정
                typeof(bool),                                           // 타입설정
                typeof(IOControllerControl),                               // 컨트롤 클래명
                new PropertyMetadata(false, OnIsMonitoringChanged));    // Default값 할당 및 기능 구현

        public static readonly DependencyProperty IOManagerProperty =
            DependencyProperty.Register(
                nameof(IOManager),
                typeof(IOManager),
                typeof(IOControllerControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty MonitoringIntervalProperty =
            DependencyProperty.Register(
                nameof(MonitoringInterval),
                typeof(int),
                typeof(IOControllerControl),
                new PropertyMetadata(0));

        public static readonly DependencyProperty IOMonitorErrorTitleProperty =
            DependencyProperty.Register(
                nameof(IOMonitorErrorTitle),
                typeof(string),
                typeof(IOControllerControl),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IOMonitorErrorMessageProperty =
            DependencyProperty.Register(
                nameof(IOMonitorErrorMessage),
                typeof(string),
                typeof(IOControllerControl),
                new PropertyMetadata(string.Empty));

        public IOManager IOManager
        {
            get => (IOManager)GetValue(IOManagerProperty);
            set => SetValue(IOManagerProperty, value);
        }
        public bool IsMonitoring
        {
            get => (bool)GetValue(IsMonitoringProperty);
            set => SetValue(IsMonitoringProperty, value);
        }
        public int MonitoringInterval
        {
            get => (int)GetValue(MonitoringIntervalProperty);
            set => SetValue(MonitoringIntervalProperty, value);
        }
        public string IOMonitorErrorTitle
        {
            get => (string)GetValue(IOMonitorErrorTitleProperty);
            set => SetValue(IOMonitorErrorTitleProperty, value);
        }
        public string IOMonitorErrorMessage
        {
            get => (string)GetValue(IOMonitorErrorMessageProperty);
            set => SetValue(IOMonitorErrorMessageProperty, value);
        }
        public ICommand OutputValueClickCommand { get; private set; }

        private Thread _monitoringThread;
        private volatile bool _isRunning;

        static IOControllerControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IOControllerControl), new FrameworkPropertyMetadata(typeof(IOControllerControl)));
        }
        public IOControllerControl()
        {
            OutputValueClickCommand = new BindingCommand<object>(OnOutputValueClick);
        }

        private void OnOutputValueClick(object obj)
        {
            if (!(obj is object[] objs) || objs.Length != 4) { return; }

            if (!(objs[0] is IOManager ioMan)) { return; }

            if (!(objs[1] is bool value)) { return; }

            if (!(objs[2] is int slotIdx)) { return; }

            if (!(objs[3] is int myIdx)) { return; }

            if (myIdx > 7)
            {
                slotIdx++;
                myIdx -= 8;
            }

            try
            {
                ioMan.WriteOutputBit(slotIdx, myIdx, !value);
            }
            catch (Exception ex)
            {
                ExceptionHandling(ex);
            }
        }


        private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IOControllerControl control = (IOControllerControl)d;
            bool isMonitoring = (bool)e.NewValue;

            if (isMonitoring)
            {
                control.StartMonitoring(control.IOManager, control.MonitoringInterval);
            }
            else
            {
                control.StopMonitoring();
            }
        }

        private void StartMonitoring(IOManager ioMan, int interval)
        {
            _isRunning = true;

            if (_monitoringThread != null && _monitoringThread.IsAlive)
            {
                return;
            }

            _monitoringThread = new Thread(() => { MonitoringThreadFunction(ioMan, interval); })
            {
                IsBackground = true
            };
            _monitoringThread.Start();
        }

        private void StopMonitoring()
        {
            _isRunning = false;
            if (_monitoringThread != null)
            {
                _monitoringThread.Join();
                _monitoringThread = null;
            }
        }

        private void MonitoringThreadFunction(IOManager ioMan, int interval)
        {
            while (_isRunning)
            {
                try
                {
                    ioMan.DInputUpdate();
                    ioMan.DOutputUpdate();

                    Thread.Sleep(interval);
                }
                catch (Exception ex)
                {
                    ExceptionHandling(ex);
                }
            }
        }

        private void ExceptionHandling(Exception ex)
        {
            string title;
            string message;
            if (ex is HandledException he)
            {
                he.GetTitleAndMessage(out title, out message);
            }
            else
            {
                title = "Unexpected error";
                message = ex.Message;
            }
            IOMonitorErrorTitle = title;
            IOMonitorErrorMessage = message;
        }
    }
}
