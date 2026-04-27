using System;
using System.Collections.Generic;
using cancrops.src.genetics;
using cancrops.src.implementations;
using cancrops.src.items;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace cancrops.src.BE
{
    public class CANBECrop: BlockEntity, ITexPositionSource
    {
        public Genome Genome;
        public AgriPlant agriPlant;
        public WeedStage weedStage = WeedStage.NONE;
        private ICoreClientAPI capi;
        public static Random rand = new Random();
        private MeshData ownMesh;
        private static readonly Dictionary<string, AssetLocation> tmpAssets = new Dictionary<string, AssetLocation>
        {
            ["e1"] = new AssetLocation("cancrops:block/e1.png"),
            ["e2"] = new AssetLocation("cancrops:block/e2.png"),
            ["e3"] = new AssetLocation("cancrops:block/e3.png"),
            ["e4"] = new AssetLocation("cancrops:block/e4.png"),
            ["e5"] = new AssetLocation("cancrops:block/e5.png"),
            ["s1"] = new AssetLocation("cancrops:block/s1.png"),
            ["s2"] = new AssetLocation("cancrops:block/s2.png"),
            ["s3"] = new AssetLocation("cancrops:block/s3.png"),
            ["s4"] = new AssetLocation("cancrops:block/s4.png"),
            ["s5"] = new AssetLocation("cancrops:block/s5.png"),
        };
        public Size2i AtlasSize => this.capi.BlockTextureAtlas.Size;
        public TextureAtlasPosition this[string textureCode]
        {
            get
            {
                if (tmpAssets.TryGetValue(textureCode, out var assetCode))
                {
                    return this.getOrCreateTexPos(assetCode);
                }

                Dictionary<string, CompositeTexture> dictionary;
                dictionary = new Dictionary<string, CompositeTexture>();
                foreach (var it in this.Block.Textures)
                {
                    dictionary.Add(it.Key, it.Value);
                }
                AssetLocation texturePath = (AssetLocation)null;
                CompositeTexture compositeTexture;
                if (dictionary.TryGetValue(textureCode, out compositeTexture))
                    texturePath = compositeTexture.Baked.BakedName;
                if ((object)texturePath == null && dictionary.TryGetValue("all", out compositeTexture))
                    texturePath = compositeTexture.Baked.BakedName;

                return this.getOrCreateTexPos(texturePath);
            }
        }
        private TextureAtlasPosition getOrCreateTexPos(AssetLocation texturePath)
        {
            TextureAtlasPosition texPos = this.capi.BlockTextureAtlas[texturePath];
            if (texPos == null)
            {
                IAsset asset = this.capi.Assets.TryGet(texturePath.Clone().WithPathPrefixOnce("textures/").WithPathAppendixOnce(".png"));
                if (asset != null)
                {
                    BitmapRef bitmap = asset.ToBitmap(this.capi);
                    this.capi.BlockTextureAtlas.GetOrInsertTexture(texturePath, out int _, out texPos, () => asset.ToBitmap(this.Api as ICoreClientAPI));
                }
                else
                {
                    this.capi.World.Logger.Warning("For render in block " + this.Block.Code?.ToString() + ", item {0} defined texture {1}, not no such texture found.", "", (object)texturePath);
                }
            }
            return texPos;
        }
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            if (api.Side == EnumAppSide.Client)
            {
                this.capi = (api as ICoreClientAPI);
            }
            if (capi != null)
            {
                this.ownMesh = GenRightMesh();
            }
        }
        public int GetCropStageWithout()
        {
            Block crop = this.GetCrop();
            if (crop != null)
            {
                return this.GetCropStage(this.GetCrop());
            }
            return 0;
        }
        internal Block GetCrop()
        {
            Block block = this.Api.World.BlockAccessor.GetBlock(this.Pos);
            if (block == null || block.CropProps == null)
            {
                return null;
            }
            return block;
        }
        internal int GetCropStage(Block block)
        {
            int stage;
            int.TryParse(block.LastCodePart(0), out stage);
            return stage;
        }
        internal MeshData GenRightMesh()
        {
            MeshData fullMesh = null;
            ownMesh?.Dispose();
            if (weedStage != WeedStage.NONE)
            {
                
                Shape weedShape = null;

                if (weedStage == WeedStage.LOW)
                {
                    weedShape = Api.Assets.TryGet("cancrops:shapes/weed-1.json").ToObject<Shape>().Clone();
                }
                else if (weedStage == WeedStage.MEDIUM)
                {
                    weedShape = Api.Assets.TryGet("cancrops:shapes/weed-2.json").ToObject<Shape>().Clone();
                }
                else if (weedStage == WeedStage.HIGH)
                {
                    weedShape = Api.Assets.TryGet("cancrops:shapes/weed-3.json").ToObject<Shape>().Clone();
                }

                if (weedShape != null)
                {
                    (Api as ICoreClientAPI).Tesselator.TesselateShape("weed", weedShape, out MeshData weedMesh, this);
                    fullMesh = weedMesh;
                }

            }
            if (fullMesh != null)
            {
                return fullMesh.Translate(new Vintagestory.API.MathTools.Vec3f(0, 0f, 0));
            }
            return fullMesh;
        }
        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            //if (base.OnTesselation(mesher, tessThreadTesselator))
            {
                //return true;
            }

            if (this.ownMesh == null)
            {
                return false;
            }
            mesher.AddMeshData(this.ownMesh, 1);
            return false;
        }
        public void TryPropagateWeed(WeedStage weedStage, CANBECrossSticks crossSticks, int depth)
        {
            if (depth > cancrops.config.weedPropagationDepth + 2)
            {
                return;
            }
            if (this.weedStage == WeedStage.HIGH)
            {
                double rnd = rand.NextDouble() * 3.45;
                int l = 0;

                foreach (var weed in cancrops.config.weedBlockCodes)
                {
                    rnd -= weed.Value;
                    if (rnd <= 0.0)
                    {
                        Block weedsBlock = this.Api.World.GetBlock(weed.Key);
                        if (weedsBlock != null)
                        {
                            this.Api.World.BlockAccessor.SetBlock(weedsBlock.BlockId, this.Pos);
                            break;
                        }
                        break;
                    }
                    else
                    {
                        l++;
                    }
                }
            }
            if (this.weedStage != WeedStage.HIGH)
            {
                int resistance = 0;
                if (this.Genome != null)
                {
                    resistance = this.Genome.Resistance.Dominant.Value;
                }
                cancrops.config.weedSpreadChancePerStage.TryGetValue((int)weedStage, out double weedChance);
                if (cancrops.config.weedResistanceByStat * resistance < weedChance + rand.Next(0, 10) / 10.0)
                {
                    this.weedStage += 1;
                    this.MarkDirty(true);
                    return;
                }
            }
            BlockPos tmpPos;
            foreach (var dir in BlockFacing.HORIZONTALS)
            {
                tmpPos = this.Pos.AddCopy(dir);
                BlockEntity blockEntityFarmland = this.Api.World.BlockAccessor.GetBlockEntity(tmpPos);
                if (cancrops.sapi.World.BlockAccessor.GetBlockEntity(this.Pos.AddCopy(dir)) is CANBECrop cbc)
                {
                    cbc.TryPropagateWeed(this.weedStage, null, depth + 1);
                }
                else if (cancrops.sapi.World.BlockAccessor.GetBlockEntity(this.Pos.AddCopy(dir)) is CANBECrossSticks cbcs)
                {
                    cbcs.TryPropagateWeed(this.weedStage, depth + 1);
                }
            }
        }
        private bool SetPlantStage(int stage)
        {
            Block block = this.GetCrop();
            if (block == null)
            {
                return false;
            }
            int currentGrowthStage = this.GetCropStage(block);

            Block nextBlock = this.Api.World.GetBlock(block.CodeWithParts(stage.ToString() ?? ""));
            if (nextBlock == null)
            {
                return false;
            }

            if (this.Api.World.BlockAccessor.GetBlockEntity(this.Pos) == null)
            {
                this.Api.World.BlockAccessor.SetBlock(nextBlock.BlockId, this.Pos);
            }
            else
            {
                this.Api.World.BlockAccessor.ExchangeBlock(nextBlock.BlockId, this.Pos);
            }
            return true;
        }
        public bool TryClipPlant(IPlayer byPlayer)
        {
            ItemSlot clipSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if(this.agriPlant == null)
            {
                return false;
            }
            if (agriPlant.Clip_products == null || this.GetCropStageWithout() < agriPlant.MinClipStage || agriPlant.MinClipStage == 0)
            {
                return false;
            }
            ItemStack clipTool = clipSlot.Itemstack;
            if (clipSlot.Empty || clipSlot.Itemstack.Item == null || clipSlot.Itemstack.Item.Tool != EnumTool.Shears)
            {
                return false;
            }
            if (!SetPlantStage(agriPlant.ClipRollbackStage))
            {
                return false;
            }

            foreach (var drop in agriPlant.Clip_products.getRandom(rand))
            {
                if (drop.Item is ItemPlantableSeed)
                {
                    ITreeAttribute genomeTree = new TreeAttribute();
                    var fertilityStat = Genome.Fertility.Dominant.Value;
                    var mutativityStat = Genome.Mutativity.Dominant.Value;
                    foreach (Gene gene in Genome)
                    {
                        ITreeAttribute geneTree = new TreeAttribute();
                        int maxStat = gene.StatName switch
                        {
                            "gain" => cancrops.config.maxGain,
                            "growth" => cancrops.config.maxGrowth,
                            "strength" => cancrops.config.maxStrength,
                            "resistance" => cancrops.config.maxResistance,
                            "fertility" => cancrops.config.maxFertility,
                            "mutativity" => cancrops.config.maxMutativity,
                            _ => gene.Dominant.Value
                        };
                        geneTree.SetInt("D", Math.Clamp(gene.Dominant.Value + RollClipDelta(mutativityStat, fertilityStat), 0, maxStat));
                        geneTree.SetInt("R", Math.Clamp(gene.Recessive.Value + RollClipDelta(mutativityStat, fertilityStat), 0, maxStat));
                        genomeTree[gene.StatName] = geneTree;
                    }
                    drop.Attributes[cancrops.config.genome_tag] = genomeTree;
                }
                this.Api.World.SpawnItemEntity(drop, new Vec3d(this.Pos.X + 0.5, this.Pos.Y + 0.5, this.Pos.Z + 0.5));
            }
            clipTool.Collectible.DamageItem(this.Api.World, byPlayer.Entity, clipSlot, 1);
            return true;
        }
        private static int RollClipDelta(int mutativityStat, int fertilityStat)
        {
            int sign = rand.Next(0, 2) * 2 - 1;
            int mut = mutativityStat > 0 ? rand.Next(mutativityStat + 1) : 0;
            int fert = fertilityStat > 0 ? rand.Next(fertilityStat + 1) : 0;
            return sign * mut + fert;
        }
        public void OnCultivating(ItemSlot slot, EntityAgent byEntity)
        {
            if(!slot.Empty && slot.Itemstack.Item is CANItemHandCultivator)
            {
                if (this.weedStage != WeedStage.NONE)
                {
                    this.weedStage = WeedStage.NONE;
                    this.MarkDirty(true);
                    if (byEntity is EntityPlayer player)
                    {
                        slot.Itemstack.Collectible.DamageItem(this.Api.World, player, slot, 1);
                    }
                }
            }
        }
        

        ///////////////////////////////////////////////////////ATTRIBUTES//////////////////////////////////////////////////////////////////////////////////// 
        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            if (tree.HasAttribute("genome"))
            {
                this.Genome = Genome.FromTreeAttribute(tree.GetTreeAttribute("genome"));
            }

            if (tree.HasAttribute("plant"))
            {
                this.agriPlant = cancrops.GetPlants()?.getPlant(tree.GetString("plant")) ?? null;
            }
            bool regenerateMesh = false;
            WeedStage newWeedStage = (WeedStage)tree.GetInt("weedStage", 0);
            if (this.weedStage != newWeedStage)
            {
                //regenerateMesh = true;
                this.weedStage = newWeedStage;
            }

            if (worldForResolving.Side == EnumAppSide.Client && this.capi != null)
            {
                regenerateMesh = true;
                if (regenerateMesh)
                {
                    ownMesh = GenRightMesh();
                }
            }
        }
        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            if (Genome != null)
            {
                tree["genome"] = this.Genome.AsTreeAttribute();
            }
            if (agriPlant != null)
            {
                tree.SetString("plant", this.agriPlant.Domain + ":" + this.agriPlant.Id);
            }
            tree.SetInt("weedStage", (int)this.weedStage);
        }
    }
}
