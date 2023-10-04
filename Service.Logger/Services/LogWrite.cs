using Serilog;
using Service.CustomException.Services.ErrorService.HandledExceptions;
using Service.Logger.ViewModels;
using Service.Logger.Views;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Service.Logger.Services
{
    public class LogWrite : LazySingleton<LogWrite>, IDisposable
    {
        private Dispatcher _currentDispatcher;

        private ErrorMessageBoxView _errView;
        private ErrorMessageBoxViewModel _errViewModel;

        // 구독한 클래스가 있다면 이벤트로 메시지를 날려줌 (UI LogBoard 사용 시 사용)
        public delegate void LogBoardUpdateEventArgs(string msg);
        public static event LogBoardUpdateEventArgs LogBoardUpdateEvent;

        /// <summary>
        /// DefaultSetting 완료 여부
        /// </summary>
        public bool IsDefaultSettingComplete { get; set; } = false;

        // TODO : 로그 버전 정보 부분.
        public string Version { get; set; } = "1.3.0";

        private LogWrite()
        {
        }
        ~LogWrite() => Dispose();

        /// <summary>
        /// 로그 기본 세팅.
        /// 내문서\프로젝트이름\LogFile 폴더에 생성
        /// + Dispatcher 안주면 일반 에러 메시지 박스 띄움
        /// </summary>
        public void DefaultSet(Dispatcher currentDispatcher = null)
        {
            _currentDispatcher = currentDispatcher;
            string doc = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string projectName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            string path = doc + "\\" + projectName + "\\LogFile";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            Log.Logger = new LoggerConfiguration().MinimumLevel.Information().Enrich.WithThreadName().WriteTo.File(
                    path + "\\.log", // 날짜.log 라는 이름으로 파일 생성
                    outputTemplate: "{Timestamp:yy-MM-dd_HH:mm:ss:fff} [{Level:u3}] <{ThreadName}> {Message:l}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day, // 일단위로 생성
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 365 // 365개만 유지
                ).CreateLogger();

            IsDefaultSettingComplete = true;

            if (_currentDispatcher != null)
            {
                // null이 아닐 경우에만 아래 내용 사용 가능.
                _currentDispatcher.Invoke(() =>
                {
                    _errView = new ErrorMessageBoxView();
                    _errViewModel = new ErrorMessageBoxViewModel();
                    _errView.DataContext = _errViewModel;
                });
            }
        }

        /// <summary>
        /// 로그파일에 정보를 남김.
        /// 가능하면 nullCheck 할것. ex) LogWrite.Instance?.Info(~) // _logWrite?.Info(~)
        /// </summary>
        /// <param name="msg">로그 메시지</param>
        /// <param name="isMessageBoxShow">메시지박스 Show 여부 확인 (Default = false)</param>
        /// <param name="isUILogBoardWrite">이벤트 날릴지 여부 확인 (구독 한 클래스 없다면 날리지 않음) (Default = true)</param>
        public void Info(string msg, bool isMessageBoxShow = false, bool isUILogBoardWrite = true)
        {
            Log.Information(msg);

            if (LogBoardUpdateEvent != null && isUILogBoardWrite)
            {
                LogBoardUpdateEvent("[INFO] - " + msg);
            }

            if (isMessageBoxShow)
                MessageBox.Show(msg, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// 로그 파일에 에러를 남김.
        /// 가능하면 nullCheck 할것. ex) LogWrite.Instance?.Error(~) // _logWrite?.Error(~)
        /// </summary>
        /// <param name="ex">발생한 Exception</param>
        /// <param name="isMessageBoxShow">메시지박스 Show 여부 확인 (Default = true)</param>
        /// <param name="isUILogBoardWrite">이벤트 날릴지 여부 확인 (구독 한 클래스 없다면 날리지 않음) (Default = true)</param>
        public void Error(Exception ex, bool isMessageBoxShow = true, bool isUILogBoardWrite = true)
        {
            string msgT = "", msgM = "";
            if (ex is HandledException hex)
            {
                hex.GetTitleAndMessage(out msgT, out msgM);
            }
            else
            {
                msgT = "UnHandled Error";
                msgM = ex.Message;
            }

            Log.Error(msgT + " : " + msgM + Environment.NewLine + ex.ToString());

            if (LogBoardUpdateEvent != null && isUILogBoardWrite)
            {
                LogBoardUpdateEvent("[ERR] - " + msgT + Environment.NewLine + msgM);
            }

            if (isMessageBoxShow)
            {
                if (_currentDispatcher != null)
                {
                    _currentDispatcher?.Invoke(() =>
                    {
                        _errViewModel.SetMessage(msgT, msgM);
                        _errView.ShowDialog();
                    });
                }
                else
                {
                    MessageBox.Show(msgT + Environment.NewLine + msgM);
                }
            }
        }

        /// <summary>
        /// 로그파일에 에러를남김.
        /// 가능하면 nullCheck 할것. ex) LogWrite.Instance?.Error(~) // _logWrite?.Error(~)
        /// </summary>
        /// <param name="ex">발생한 Exception</param>
        /// <param name="isMessageBoxShow">메시지박스 Show 여부 확인 (Default = true)</param>
        /// <param name="isUILogBoardWrite">이벤트 날릴지 여부 확인 (구독 한 클래스 없다면 날리지 않음) (Default = true)</param>
        public void Error(string msg, bool isMessageBoxShow = true, bool isUILogBoardWrite = true)
        {
            Log.Error(msg);

            if (LogBoardUpdateEvent != null && isUILogBoardWrite)
            {
                LogBoardUpdateEvent("[ERR] - " + msg);
            }

            if (isMessageBoxShow)
            {
                if (_currentDispatcher != null)
                {
                    _currentDispatcher?.Invoke(() =>
                    {
                        _errViewModel.SetMessage("Error", msg);
                        _errView.ShowDialog();
                    });
                }
                else
                {
                    MessageBox.Show("Error" + Environment.NewLine + msg);
                }
            }
        }

        public void Dispose()
        {
            try
            {
                _currentDispatcher?.Invoke(() =>
                {
                    _errView?.Close();
                    _errView = null;
                });
            }
            catch { }
        }
    }
}
