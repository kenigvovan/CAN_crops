using ProtoBuf.Meta;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using cancrops.src.utility;
using Vintagestory.Common;
using cancrops.src.templates;

namespace cancrops.src.implementations
{
    public class AgriPlant : IAgriRegisterable
    {
        public bool Enabled { get; set; }
        public string Domain { get; set; }
        public string Id { get; set; }
        public float GrowthMultiplier { get; set; }
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
        public AgriProductList Products { get; set; }
        public AgriProductList Clip_products { get; set; }
        public AgriRequirement Requirement { get; set; }

        public AgriPlant()
        {

        }
        public AgriPlant(JsonAgriPlant jsonAgriPlant)
        {
            this.Enabled = jsonAgriPlant.Enabled;
            this.Domain = jsonAgriPlant.Domain;
            this.Id = jsonAgriPlant.Id;
            this.GrowthMultiplier = jsonAgriPlant.GrowthMultiplier;
            this.AllowUnderGround = jsonAgriPlant.AllowUnderGround;
            this.Cloneable = jsonAgriPlant.Cloneable;
            this.SpreadChance = jsonAgriPlant.SpreadChance;
            this.SeedDropChance = jsonAgriPlant.SeedDropChance;
            this.SeedDropBonus = jsonAgriPlant.SeedDropBonus;
            this.AllowCloning = jsonAgriPlant.AllowCloning;
            this.AllowSourceStage = jsonAgriPlant.AllowSourceStage;
            this.ClipSeedsHaveStats = jsonAgriPlant.ClipSeedsHaveStats;
            this.MinClipStage = jsonAgriPlant.MinClipStage;
            this.ClipRollbackStage = jsonAgriPlant.ClipRollbackStage;
            if (jsonAgriPlant.Products != null)
            {
                this.Products = new AgriProductList();
                foreach (var product in jsonAgriPlant.Products.getAll())
                {
                    if (product.ItemClass == EnumItemClass.Item)
                    {
                        Item itemTmp = cancrops.sapi.World.GetItem(new AssetLocation(product.CollectibleCode));
                        if (itemTmp == null)
                        {
                            continue;
                        }
                        var bdis = new BlockDropItemStack(new ItemStack(itemTmp, 1));
                        bdis.Quantity = new Vintagestory.API.MathTools.NatFloat(product.Avg, product.Var, Vintagestory.API.MathTools.EnumDistribution.VERYNARROWGAUSSIAN);
                        bdis.LastDrop = product.LastDrop;
                        this.Products.AddProduct(bdis);
                    }
                }
            }
            if (jsonAgriPlant?.Clip_products != null)
            {
                this.Clip_products = new AgriProductList();
                foreach (var product in jsonAgriPlant.Clip_products.getAll())
                {
                    if (product.ItemClass == EnumItemClass.Item)
                    {
                        Item itemTmp = cancrops.sapi.World.GetItem(new AssetLocation(product.CollectibleCode));
                        if (itemTmp == null)
                        {
                            continue;
                        }
                        var bdis = new BlockDropItemStack(new ItemStack(itemTmp, 1));
                        bdis.Quantity = new Vintagestory.API.MathTools.NatFloat(product.Avg, product.Var, Vintagestory.API.MathTools.EnumDistribution.VERYNARROWGAUSSIAN);
                        bdis.LastDrop = product.LastDrop;
                        this.Clip_products.AddProduct(bdis);
                    }
                    else
                    {
                        Block itemTmp = cancrops.sapi.World.GetBlock(new AssetLocation(product.CollectibleCode));
                        if (itemTmp == null)
                        {
                            continue;
                        }
                        var bdis = new BlockDropItemStack(new ItemStack(itemTmp, 1));
                        bdis.Quantity = new Vintagestory.API.MathTools.NatFloat(product.Avg, product.Var, Vintagestory.API.MathTools.EnumDistribution.VERYNARROWGAUSSIAN);
                        bdis.LastDrop = product.LastDrop;
                        this.Clip_products.AddProduct(bdis);
                    }
                }
            }
            if (jsonAgriPlant.Requirement != null)
            {
                this.Requirement = new AgriRequirement();
                this.Requirement.LightToleranceFactor = jsonAgriPlant.Requirement.LightToleranceFactor;
                this.Requirement.MinLight = jsonAgriPlant.Requirement.MinLight;
                this.Requirement.MaxLight = jsonAgriPlant.Requirement.MaxLight;
                this.Requirement.LightLevelType = jsonAgriPlant.Requirement.LightLevelType;
                if (jsonAgriPlant.Requirement.Conditions != null)
                {
                    this.Requirement.Conditions = new List<AgriBlockCondition>();
                    foreach (var it in jsonAgriPlant.Requirement.Conditions)
                    {
                        Block tmpBlock = cancrops.sapi.World.GetBlock(new AssetLocation(it.BlockName));
                        if (tmpBlock != null)
                        {
                            this.Requirement.Conditions.Add(new AgriBlockCondition(tmpBlock, it.Amount,
                                new Vintagestory.API.MathTools.BlockPos(it.MinX, it.MinY, it.MinZ, 0),
                                new Vintagestory.API.MathTools.BlockPos(it.MaxX, it.MaxY, it.MaxZ, 0)
                                ));
                        }
                    }
                }
            }
        }

        public void getHarvestProducts(List<ItemStack> products, Random rand)
        {
            products = this.Products.getRandom(rand);
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
