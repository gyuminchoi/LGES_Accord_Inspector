using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Camera.Models
{
    public enum ECameraGetSetType
    {
        VFGEnum,
        VFGString,
        VFGInt,
        VFGFloat,
        VFGBool,
        VFGCmd,


        MultiCamUIntUInt,
        MultiCamStrUInt,
        MultiCamUIntInt,
        MultiCamStrInt,
        MultiCamUIntLong,
        MultiCamStrLong,
        MultiCamUIntDouble,
        MultiCamStrDouble,
        MultiCamUIntString,
        MultiCamStrString,
        MultiCamUIntIntPtr,
        MultiCamStrIntPtr,
    }
}
