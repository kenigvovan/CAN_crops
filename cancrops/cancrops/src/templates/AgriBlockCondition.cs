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
        public int MinX { get; set; }
        public int MinY { get; set; }
        public int MinZ { get; set; }
        public int MaxX { get; set; }
        public int MaxY { get; set; }
        public int MaxZ { get; set; }

    }
}
