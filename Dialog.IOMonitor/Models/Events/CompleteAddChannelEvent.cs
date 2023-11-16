using Dialog.IOMonitor.Models.IOData;
using Prism.Events;
using System.Collections.Generic;

namespace Dialog.IOMonitor.Models.Events
{
    public class CompleteAddChannelEvent : PubSubEvent<List<SignalGraphData>> { }
}
