using MahApps.Metro.Controls;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Dialog.MahAppsThema.Views
{
    /// <summary>
    /// MahAppsThemaDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MahAppsThemaDialog : MetroWindow, IDialogWindow
    {
        private IDialogWindow _dialogWindow;
        public MahAppsThemaDialog()
        {
            InitializeComponent();
        }

        public IDialogResult Result { get; set; }
    }
}
