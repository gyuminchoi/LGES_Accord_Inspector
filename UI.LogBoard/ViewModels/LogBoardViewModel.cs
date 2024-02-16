﻿using Prism.Commands;
using Prism.Mvvm;
using Service.Logger.Services;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UI.LogBoard.ViewModels
{
    public class LogBoardViewModel : BindableBase
    {
        private object _systemLogLock = new object();
        private LogWrite _logWrite = LogWrite.Instance;
        public AutoDeleteObservableCollection<string> LogBoard { get; set; } = new AutoDeleteObservableCollection<string>(10);

        public DelegateCommand BtnLogResetClickCommand => new DelegateCommand(OnResetLogBoard);
        public DelegateCommand LoadedCommand => new DelegateCommand(OnLoaded);

        public LogBoardViewModel() { }

        private void OnLoaded()
        {
            LogWrite.LogBoardUpdateEvent += OnSystemLogUpdate;
            BindingOperations.EnableCollectionSynchronization(LogBoard, _systemLogLock);
        }

        private void OnSystemLogUpdate(string msg)
        {
            lock (_systemLogLock)
            {
                LogBoard.Add($"{DateTime.Now}{Environment.NewLine}{msg}");
            }
        }

        private void OnResetLogBoard() 
        {
            lock (_systemLogLock)
            {
                LogBoard.Clear();
            }
        } 
    }
}
