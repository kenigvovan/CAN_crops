using Vintagestory.API.Common;

namespace cancrops.src.templates
{
    public class JsonAgriProduct
    {
        public string CollectibleCode { get; set; }
        public EnumItemClass ItemClass { get; set; }
        public float Avg { get; set; }
        public float Var { get; set; }
        public bool LastDrop { get; set; }
        public JsonAgriProduct()
        {
            
        }
    }
}
