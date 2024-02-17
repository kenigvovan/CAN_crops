using cancrops.src.templates;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace cancrops.src.templates
{
    public class AgriProduct
    {
        public string CollectibleCode { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }

        public double Chance { get; set; }

        public bool Required { get; set; }

        public AgriProduct()
        {

        }
     

        public bool shouldDrop(Random rand)
        {
            return Chance > rand.NextDouble();
        }

        public int getAmount(Random rand)
        {
            return Min + rand.Next(Max - Min + 1);
        }

        public ItemStack convertSingle(AgriProduct token, Random random)
        {
            var itemStack = new ItemStack(cancrops.sapi.World.GetItem(new AssetLocation(token.CollectibleCode)));
            if (itemStack != null)
            {
                itemStack.StackSize = Math.Max(1, getAmount(random));
            }
            return itemStack;
        }
    }
}
