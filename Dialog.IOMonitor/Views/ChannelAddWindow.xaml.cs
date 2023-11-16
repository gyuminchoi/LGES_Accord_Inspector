using Dialog.IOMonitor.Models.IOData;
using Dialog.IOMonitor.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Dialog.IOMonitor.Views
{
    /// <summary>
    /// ChannelAddWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChannelAddWindow : Window
    {
        public ChannelAddWindow()
        {
            InitializeComponent();
        }

        private void ChannelCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewerBitData data = (sender as ListView).SelectedItem as ViewerBitData;

            (DataContext as ChannelAddViewModel).ChangeSelection(data);
        }
    }
}
