using Dialog.IOMonitor.Models.IOData;
using Prism.Events;

namespace Dialog.IOMonitor.Models.Events
{
    public class ChannelAddEvent : PubSubEvent<SelectedChannelData> { }
}
