using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using cancrops.src.utility;
using cancrops.src.templates;
using Vintagestory.API.Util;

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
        public float LightSensitivity { get; set; } = 1f;
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
            this.LightSensitivity = jsonAgriPlant.LightSensitivity;
            if (jsonAgriPlant.Products != null)
            {
                this.Products = new AgriProductList();
                foreach (var product in jsonAgriPlant.Products.getAll())
                {
                    if (product.ItemClass == EnumItemClass.Item)
                    {
                        Item itemTmp = cancrops.api.World.GetItem(new AssetLocation(product.CollectibleCode));
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
                        Item itemTmp = cancrops.api.World.GetItem(new AssetLocation(product.CollectibleCode));
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
                        Block itemTmp = cancrops.api.World.GetBlock(new AssetLocation(product.CollectibleCode));
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
                this.Requirement.RequirementFromStage = jsonAgriPlant.Requirement.RequirementFromStage;
                if (jsonAgriPlant.Requirement.Conditions != null)
                {
                    this.Requirement.Conditions = new List<AgriBlockCondition>();
                    foreach (var it in jsonAgriPlant.Requirement.Conditions)
                    {
                        var blockIds = ResolveBlockIds(it.BlockName);
                        if (blockIds.Count > 0)
                        {
                            this.Requirement.Conditions.Add(new AgriBlockCondition(blockIds, it.Amount,
                                new Vintagestory.API.MathTools.BlockPos(it.MinX, it.MinY, it.MinZ, 0),
                                new Vintagestory.API.MathTools.BlockPos(it.MaxX, it.MaxY, it.MaxZ, 0)
                                ));
                        }
                    }
                }
            }
        }

        // Resolves a block-name (possibly with wildcards like "game:ore-iron-*") to a set
        // of block ids. Uses the API's built-in IWorldAccessor.SearchBlocks for wildcard
        // patterns — same machinery vanilla uses, so behaviour matches recipe matching.
        private static HashSet<int> ResolveBlockIds(string blockName)
        {
            var ids = new HashSet<int>();
            if (string.IsNullOrEmpty(blockName)) return ids;
            var loc = new AssetLocation(blockName);
            if (blockName.Contains('*'))
            {
                Block[] matches = cancrops.api.World.SearchBlocks(loc);
                if (matches != null)
                {
                    foreach (Block b in matches)
                    {
                        if (b != null && b.Id != 0) ids.Add(b.Id);
                    }
                }
            }
            else
            {
                Block b = cancrops.api.World.GetBlock(loc);
                if (b != null) ids.Add(b.Id);
            }
            cancrops.api.Logger.VerboseDebug("[cancrops] ResolveBlockIds '{0}' -> {1} blocks", blockName, ids.Count);
            return ids;
        }

        public void getHarvestProducts(List<ItemStack> products, Random rand)
        {
            var randomProducts = this.Products.getRandom(rand);
            products.Clear();
            if (randomProducts != null)
            {
                products.AddRange(randomProducts);
            }
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
