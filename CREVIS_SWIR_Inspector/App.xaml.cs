using CREVIS_SWIR_Inspector.Main.Services.Bootstrappers;
using Service.Logger.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CREVIS_SWIR_Inspector.Main
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        private Mutex _mutex = null;
        private LogWrite _logWrite = LogWrite.Instance;
        protected override void OnStartup(StartupEventArgs e)
        {
            _logWrite.DefaultSet();

            Thread.CurrentThread.Name = "MainThread";
            // 중복실행방지
            var mutexName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            var isCreatedNew = false;
            try
            {
                _mutex = new Mutex(true, mutexName, out isCreatedNew);

                if (isCreatedNew)
                {
                    // 관리되지 않는 Exception Event
                    AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                    base.OnStartup(e);
                }
                else
                {
                    MessageBox.Show("Application already started.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                    _logWrite?.Info("Application already started");
                    Current.Shutdown();
                }
            }
            catch (Exception)
            {
                //MessageBox.Show("Application RecipeLoad Error.\r\n" + err.Message);
                _logWrite?.Error("Application Load Error.", true, false);
                Current.Shutdown();
            }

            // Prism BootStrapper 시작
            _logWrite?.Info("Bootstrapper Start!", false, false);
            Bootstrapper bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string msg = "UnHandled Exception!" + Environment.NewLine + e.ExceptionObject.ToString();
            try
            {
                LogWrite.Instance.Error(msg);
            }
            catch
            {
                MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Environment.Exit(0);
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }
    }
}
