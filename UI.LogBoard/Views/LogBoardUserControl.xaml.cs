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

namespace UI.LogBoard.Views
{
    /// <summary>
    /// LogBoardUserControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LogBoardUserControl : UserControl
    {
        private bool _autoScroll;

        public LogBoardUserControl()
        {
            InitializeComponent();
        }
        private void LogScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange == 0)
            {
                if (LogScrollViewer.VerticalOffset == LogScrollViewer.ScrollableHeight) _autoScroll = true;
                else _autoScroll = false;
            }

            // 스크롤 진행
            if (_autoScroll && e.ExtentHeightChange != 0) LogScrollViewer.ScrollToVerticalOffset(LogScrollViewer.ExtentHeight);
        }
    }
}
