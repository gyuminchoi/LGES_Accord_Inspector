using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Service.Database.Services;
using Service.Logger.Services;
using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;

namespace Dialog.History.ViewModels
{
    public class HistoryViewModel : BindableBase, IDialogAware
    {
        private ISQLiteManager _sqliteManager;
        private LogWrite _logWrite = LogWrite.Instance;
        private string _parcelBarcode;
        private string _productBarcode;
        private DateTime _startDate;
        private DateTime _endDate;
        private string _debugLog;
        private DataView _searchedData;
        private bool _isExportCSVEnabled;
        public string Title => "History";
        public event Action<IDialogResult> RequestClose;

        public string ParcelBarcode { get => _parcelBarcode; set => SetProperty(ref _parcelBarcode, value); }
        public string ProductBarcode { get => _productBarcode; set => SetProperty(ref _productBarcode, value); }
        public DateTime StartDate { get => _startDate; set => SetProperty(ref _startDate, value); }
        public DateTime EndDate { get => _endDate; set => SetProperty(ref _endDate, value); }
        public DataView SearchedData { get => _searchedData; set => SetProperty(ref _searchedData, value); }
        public string DebugLog { get => _debugLog; set => SetProperty(ref _debugLog, value); }
        public bool IsExportCSVEnabled { get => _isExportCSVEnabled; set => SetProperty(ref _isExportCSVEnabled, value); }

        public DelegateCommand<object> BtnImageOpenCommand => new DelegateCommand<object>(OnImageOpen);
        public DelegateCommand BtnSearchCommand => new DelegateCommand(OnSearch);
        public DelegateCommand BtnExportCSVCommand => new DelegateCommand(OnExportCSV);

        public HistoryViewModel(ISQLiteManager sqliteManager)
        {
            _sqliteManager = sqliteManager;
        }

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            StartDate = DateTime.Now.AddDays(-1);
            EndDate = DateTime.Now.AddDays(1);

            _sqliteManager.ExecuteCommandComplete += OnUpdateDebugLog;
        }

        private void OnUpdateDebugLog(object sender, string e) => DebugLog = e;

        private void OnImageOpen(object imagePath)
        {
            try
            {
                string path = (string)imagePath;

                if (!File.Exists(path))
                {
                    _logWrite.Info("Do not Exists Image", true, false);
                    return;
                }

                Process.Start(path);
            }
            catch (Exception err)
            {
                _logWrite?.Error(err);
            }
        }

        private void OnSearch()
        {
            try
            {
                DataTable dt = _sqliteManager.Search(ParcelBarcode, ProductBarcode, StartDate, EndDate);
                SearchedData = dt.DefaultView;
                
                if(SearchedData.Count > 0)
                    IsExportCSVEnabled = true;
                if(SearchedData.Count < 1)
                    IsExportCSVEnabled = false;
            }
            catch (Exception err)
            {
                _logWrite?.Error(err);
            }
        }

        private void OnExportCSV()
        {
            _sqliteManager.ExportCSV(SearchedData.ToTable());
        }
    }
}
