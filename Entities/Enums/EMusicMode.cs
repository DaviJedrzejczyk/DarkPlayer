using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Enums
{
    public enum EMusicMode
    {
        [EnumMember(Value = "CONTINUOUS")]
        CONTINUOUS,

        [EnumMember(Value = "SHUFFLE")]
        SHUFFLE,

        [EnumMember(Value = "REPLAY_UNIQUE")]
        REPLAY_UNIQUE
    }
}
