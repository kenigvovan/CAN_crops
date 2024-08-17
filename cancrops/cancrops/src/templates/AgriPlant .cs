using ProtoBuf.Meta;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using cancrops.src.utility;
using cancrops.src.templates;
using Vintagestory.Common;

namespace cancrops.src.templates
{
    public class AgriPlant: IAgriRegisterable
    {
        public bool Enabled { get; set; }
        public string Domain { get; set; }
        public string Id { get; set; }
        public float GrowthMultiplier {  get; set; }
        public bool AllowUnderGround { get; set; }
        public bool Cloneable { get; set; }
        public double SpreadChance { get; set; }
        public double SeedDropChance { get; set; }
        public double SeedDropBonus { get; set; }
        public bool AllowCloning { get; set; }
        public int AllowSourceStage { get; set; }

        public AgriProductList Products { get; set; }
        public AgriProductList Clip_products { get; set; }
        public AgriRequirement Requirement { get; set; }

        public AgriPlant()
        {
            
        }

        public void getHarvestProducts(List<ItemStack> products, Random rand)
        {
            products = Products
                .getRandom(rand)
                    .ConvertAll(product => product.convertSingle(product, rand))
                    .Where(it => it != null).ToList();
        }

        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }

        public string getId()
        {
            return Id;
        }
    }
}
