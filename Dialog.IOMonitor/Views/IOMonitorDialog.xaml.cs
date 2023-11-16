using MahApps.Metro.IconPacks;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dialog.IOMonitor.Views
{
    /// <summary>
    /// IOMonitorDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class IOMonitorDialog : UserControl
    {
        public IOMonitorDialog()
        {
            InitializeComponent();
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e) => AllDeleteIcon.Kind = PackIconMaterialKind.DeleteEmpty;

        private void Button_MouseLeave(object sender, MouseEventArgs e) => AllDeleteIcon.Kind = PackIconMaterialKind.Delete;
    }
}
