using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace cancrops.src.implementations
{
    public class JsonAgriProductList
    {
        public List<JsonAgriProduct> Products { get; set; }
        public JsonAgriProductList()
        {
            Products = new List<JsonAgriProduct>();
        }
        public JsonAgriProductList(List<JsonAgriProduct> products)
        {
            Products = products;
        }
        public List<JsonAgriProduct> getAll()
        {
            return new List<JsonAgriProduct>(Products);
        }
    }
}
