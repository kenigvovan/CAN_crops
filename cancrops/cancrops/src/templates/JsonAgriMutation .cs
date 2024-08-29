using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Vintagestory.API.Common;
using cancrops.src.templates;

namespace cancrops.src.implementations
{
    public class JsonAgriMutation
    {
        public bool Enabled { get; set; }
        public double Chance { get; set; }
        public string Child { get; set; }
        public string Parent1 { get; set; }
        public string Parent2 { get; set; }
        //public List<AgriMutationCondition> Conditions { get; set; }
        public JsonAgriMutation()
        {

        }
        public bool isChild(string child)
        {
            return Child.Equals(child);
        }
        public bool isParent(string parent)
        {
            return Parent1.Equals(parent) || Parent2.Equals(parent);
        }
    }
}
