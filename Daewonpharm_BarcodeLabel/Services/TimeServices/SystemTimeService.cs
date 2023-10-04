using Prism.Mvvm;
using Service.Logger.Services;
using System;
using System.Windows.Threading;

namespace PrismTemplate.Services.TimeServices
{
    public class SystemTimeService : BindableBase, IDisposable
    {
        private DispatcherTimer _dispatcherTimer;
        private string _currentTime;

        public string CurrentTime 
        {
            get => _currentTime;
            set => SetProperty(ref _currentTime, value);
        }

        public SystemTimeService() { }
        ~SystemTimeService() => Dispose();


        /// <summary>
        /// DispatcherTimer 인터벌 99ms로 실행.
        /// </summary>
        public void DispatcherTimerStart()
        {
            try
            {
                _dispatcherTimer = new DispatcherTimer();
                _dispatcherTimer.Tick += new System.EventHandler(OnDispatcherTimer);
                _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(99);
                _dispatcherTimer.Stop();
                _dispatcherTimer.Start();
            }
            catch (Exception ex) { LogWrite.Instance?.Error(ex); }
        }

        private void OnDispatcherTimer(object sender, EventArgs e)
        {
            CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public void Dispose()
        {
            if(_dispatcherTimer != null)
            {
                _dispatcherTimer.Stop();
                _dispatcherTimer = null;
            }
        }
    }
}
