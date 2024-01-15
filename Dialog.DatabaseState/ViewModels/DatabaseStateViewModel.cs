using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Service.Database.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dialog.DatabaseState.ViewModels
{
    public class DatabaseStateViewModel : BindableBase, IDialogAware
    {
        private ISQLiteManager _sqliteManager;
        private ConnectionState _connSatae;

        public event Action<IDialogResult> RequestClose;

        public ConnectionState ConnState { get => _connSatae; set => SetProperty(ref _connSatae, value); }

        public DelegateCommand BtnConnectionCheckCommand => new DelegateCommand(OnDBConnectionCheck);
        public DelegateCommand BtnReconnectCommand => new DelegateCommand(OnDBReconnect);

        public string Title => "Database State";

        public DatabaseStateViewModel(ISQLiteManager sqliteManager)
        {
            _sqliteManager = sqliteManager;
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            ConnState = _sqliteManager.CheckDBState();
        }

        private void OnDBReconnect()
        {
            _sqliteManager.Reconnect();
        }

        private void OnDBConnectionCheck()
        {
            ConnState = _sqliteManager.CheckDBState();
        }

        
    }
}
