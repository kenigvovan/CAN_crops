using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cancrops.src.implementations
{
    public class AgriRequirement
    {
        public int LightLevelType { get; set; }
        public int MinLight { get; set; }
        public int MaxLight { get; set; }
        public double LightToleranceFactor { get; set; }
        public List<AgriBlockCondition> Conditions { get; set; }
    }
}
