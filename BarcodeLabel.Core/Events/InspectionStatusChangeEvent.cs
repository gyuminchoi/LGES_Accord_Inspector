﻿using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeLabel.Core.Events
{
    public class InspectionStatusChangeEvent : PubSubEvent<bool> { }
}
