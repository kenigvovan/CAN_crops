using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cancrops.src.templates
{
    public class AgriRequirement
    {
        public int Min_light { get; set; }
        public int Max_light { get; set; }
        public double Light_tolerance_factor { get; set; }

        public List<AgriBlockCondition> Conditions { get; set; }
    }
}
