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

namespace cancrops.src.templates
{
    public class AgriMutation : IComparable
    {
        public bool Enabled { get; set; }
        public double Chance { get; set; }

        public string Child { get; set; }

        public string Parent1 { get; set; }
        public string Parent2 { get; set; }

        //public List<AgriMutationCondition> Conditions { get; set; }

        public AgriMutation()
        {

        }

        public bool isChild(string child)
        {
            return this.Child.Equals(child);
        }

        public bool isParent(string parent)
        {
            return Parent1.Equals(parent) || Parent2.Equals(parent);
        }

        public AgriPlant getChild()
        {
            return cancrops.GetPlants().getPlant(Child.Split(':')[1]);
        }

        public AgriPlant getParent1()
        {
            return cancrops.GetPlants().getPlant(Parent1);
        }

        public AgriPlant getParent2()
        {
            return cancrops.GetPlants().getPlant(Parent2);
        }

        public bool randomMutate(Random rand)
        {
            return Chance > rand.NextDouble();
        }
      
        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
