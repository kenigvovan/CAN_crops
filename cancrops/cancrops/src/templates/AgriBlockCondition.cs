using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cancrops.src.templates
{
    public class AgriBlockCondition
    {
        public string BlockName { get; set; }
        public int Strength { get; set; }
        public int Amount { get; set; }
        public int Min_x { get; set; }
        public int Min_y { get; set; }
        public int Min_z { get; set; }
        public int Max_x { get; set; }
        public int Max_y { get; set; }
        public int Max_z { get; set; }

    }
}
