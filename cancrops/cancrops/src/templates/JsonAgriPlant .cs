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
using cancrops.src.implementations;

namespace cancrops.src.templates
{
    public class JsonAgriPlant : IAgriRegisterable
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
        public bool ClipSeedsHaveStats { get; set; }
        public int MinClipStage { get; set; }
        public int ClipRollbackStage { get; set; }
        public JsonAgriProductList Products { get; set; }
        public JsonAgriProductList Clip_products { get; set; }
        public JsonAgriRequirement Requirement { get; set; }
        public JsonAgriPlant()
        {
            
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
