using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dialog.SelectionDirectiory.ViewModels
{
    public class SelectionDirectoryViewModel : BindableBase, IDialogAware
    {
        public string Title => "SelectionDirectiory";

        public event Action<IDialogResult> RequestClose;

        public DelegateCommand LoadedCommand => new DelegateCommand(OnLoaded);

        private void OnLoaded()
        {
        }

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }
    }
}
