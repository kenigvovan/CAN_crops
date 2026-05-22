using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using Vintagestory.API.Datastructures;

namespace cancrops.src.utility
{
    [ProtoContract]
    public class ConfigUpdateValuesPacket
    {
        [ProtoMember(1)]
        public float coldResistanceByStat;
        [ProtoMember(2)]
        public float heatResistanceByStat;
        [ProtoMember(3)]
        public bool hiddenGain;
        [ProtoMember(4)]
        public bool hiddenGrowth;
        [ProtoMember(5)]
        public bool hiddenStrength;
        [ProtoMember(6)]
        public bool hiddenResistance;
        [ProtoMember(7)]
        public bool hiddenFertility;
    }
}
