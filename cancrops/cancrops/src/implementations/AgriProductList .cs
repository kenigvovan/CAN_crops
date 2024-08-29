using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace cancrops.src.implementations
{
    public class AgriProductList
    {
        public List<BlockDropItemStack> Products { get; set; }

        public AgriProductList()
        {
            Products = new List<BlockDropItemStack>();
        }
        public void AddProduct(BlockDropItemStack item)
        {
            this.Products.Add(item);
        }
        public AgriProductList(List<BlockDropItemStack> products)
        {
            Products = products;
        }
        public List<BlockDropItemStack> getAll()
        {
            return new List<BlockDropItemStack>(Products);
        }
        public List<ItemStack> getRandom(Random rand)
        {
            List<ItemStack> produce = new List<ItemStack>();

            foreach (var product in Products)
            {
                ItemStack loot = product.GetNextItemStack();
                if (loot != null)
                {
                    produce.Add(loot);
                    if (product.LastDrop)
                    {
                        break;
                    }
                }
            }
            return produce;
        }
    }
}
