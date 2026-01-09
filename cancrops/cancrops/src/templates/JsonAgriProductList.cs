using System.Collections.Generic;

namespace cancrops.src.templates
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
