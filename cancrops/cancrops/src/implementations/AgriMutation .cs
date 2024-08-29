using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace cancrops.src.implementations
{
    public class AgriMutation : IComparable
    {
        public double Chance { get; set; }
        public string Child { get; set; }
        public string Parent1 { get; set; }
        public string Parent2 { get; set; }
        public AgriMutation()
        {

        }
        public AgriMutation(JsonAgriMutation jsonAgriMutation)
        {
            this.Chance = jsonAgriMutation.Chance;
            this.Child = jsonAgriMutation.Child;
            this.Parent1 = jsonAgriMutation.Parent1;
            this.Parent2 = jsonAgriMutation.Parent2;
        }
        public bool isChild(string child)
        {
            return Child.Equals(child);
        }
        public bool isParent(string parent)
        {
            return Parent1.Equals(parent) || Parent2.Equals(parent);
        }
        public AgriPlant getChild()
        {
            return cancrops.GetPlants().getPlant(Child);
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
        public override int GetHashCode()
        {
            int hash = 13;
            if(this.Parent1.CompareTo(this.Parent2) > 0)
            {
                hash = this.Parent1.GetHashCode() + this.Parent2.GetHashCode();
            }
            else
            {
                hash = this.Parent2.GetHashCode() + this.Parent1.GetHashCode();
            }
            hash = (hash * 7) + this.Child.GetHashCode();
            return hash;
        }
    }
}
