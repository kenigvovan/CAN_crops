using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace cancrops.src.implementations
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
