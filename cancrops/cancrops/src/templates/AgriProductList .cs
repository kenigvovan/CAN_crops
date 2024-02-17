using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace cancrops.src.templates
{
    public class AgriProductList
    {

        public List<AgriProduct> Products { get; set; }

        public AgriProductList()
        {
            Products = new List<AgriProduct>();
        }

        public AgriProductList(List<AgriProduct> products)
        {
            this.Products = products;
        }

        public List<AgriProduct> getAll()
        {
            return new List<AgriProduct>(Products);
        }

        public List<AgriProduct> getRandom(Random rand)
        {
            List<AgriProduct> produce = new List<AgriProduct>();
            Products.ForEach((product) =>
            {
                if (product.shouldDrop(rand))
                {
                    produce.Add(product);
                }
            });
            return produce;
        }
    }
}
