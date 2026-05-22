using System;
using System.Collections.Generic;
using System.Reflection;
using cancrops.src.blocks;
using cancrops.src.genetics;
using cancrops.src.implementations;
using cancrops.src.items;
using cancrops.src.utility;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace cancrops.src.BE
{
    public class CANBECrossSticks: BlockEntity
    {
        protected static Random rand = new Random();
        MeshData currentRightMesh;
        public WeedStage weedStage = WeedStage.NONE;
        private ICoreClientAPI capi;
        public string type = "oak";
        private MeshData ownMesh;
        public int weedProtectionTicks = 0;
        public double totalHoursLastWeed = 0;
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            if (api is ICoreServerAPI)
            {
                this.RegisterGameTickListener(new Action<float>(this.Update), 20000 + rand.Next(4000), 0);
            }
            this.MarkDirty(true);
            if (api.Side == EnumAppSide.Client)
            {
               /* if (this.currentRightMesh == null)
                {
                    this.currentRightMesh = this.GenRightMesh();
                    this.MarkDirty(true);
                }*/
                this.capi = (api as ICoreClientAPI);
                
                this.loadOrCreateMesh();
                
            }
        }
        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if (base.OnTesselation(mesher, tessThreadTesselator))
            {
                //return true;
            }
            if (this.ownMesh == null)
            {
                return true;
            }

            mesher.AddMeshData(this.ownMesh, 1);
            return true;
        }
        public void Update(float dt)
        {
            List<CANBECrop> neighbours = new();
            BlockPos tmpPos;
            foreach (var dir in BlockFacing.HORIZONTALS)
            {
                tmpPos = this.Pos.AddCopy(dir);
                BlockEntity blockEntityFarmland = this.Api.World.BlockAccessor.GetBlockEntity(tmpPos);
                if (cancrops.sapi.World.BlockAccessor.GetBlockEntity(this.Pos.AddCopy(dir)) is CANBECrop cbc)
                {
                    if (cbc is not null)
                    {
                        neighbours.Add(cbc);
                    }
                }
            }
            if (this.weedStage == WeedStage.NONE && neighbours.Count > 0 && cancrops.GetAgriMutationHandler().handleCrossBreedTick(this, neighbours, rand))
            {
                //MinecraftForge.EVENT_BUS.post(new AgriCropEvent.Grow.Cross.Post(this));
            }  
            if(cancrops.config.weedSpreadingActivated)
            {
                this.TryPropagateWeed(this.weedStage);
            }
        }
        public bool spawnGenome(Genome genome, AgriPlant agriPlant)
        {
            this.setGenomeImpl(genome, agriPlant);
            return true;
        }
        protected void setGenomeImpl(Genome genome, AgriPlant agriPlant)
        {

            Block block = this.Api.World.GetBlock(new AssetLocation(agriPlant.Domain + ":crop-" + agriPlant.Id + "-1"));

            if (block == null)
            {
                return;
            }
            var seeds = CommonUtils.GetSeedItemStackFromFarmland(genome, agriPlant);
            DummySlot itemSlot = new DummySlot(seeds);
            this.Api.World.BlockAccessor.SetBlock(block.BlockId, this.Pos);
            CropBehavior[] behaviors = block.CropProps.Behaviors;
            var sel = new BlockSelection(this.Pos, BlockFacing.DOWN, block);
            var farmBe = this.Api.World.BlockAccessor.GetBlockEntity(sel.Position.DownCopy());
            if(farmBe is not BlockEntityFarmland)
            {
                return;
            }

            //(farmBe as BlockEntityFarmland).totalHoursForNextStage = this.Api.World.Calendar.TotalHours + this.GetHoursForNextStage();
            var field = typeof(BlockEntityFarmland).GetField(
                "totalHoursForNextStage",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            field.SetValue(farmBe, this.Api.World.Calendar.TotalHours + (farmBe as BlockEntityFarmland).GetHoursForNextStage());
            for (int i = 0; i < behaviors.Length; i++)
            {
                behaviors[i].OnPlanted(this.Api, itemSlot, null, sel);
            }
            this.MarkDirty(true);
        }
        public void TryPropagateWeed(WeedStage weedStage, int depth = 1)
        {
            if(weedProtectionTicks > 0)
            {
                weedProtectionTicks--;
                return;
            }
            if(depth > cancrops.config.weedPropagationDepth + 1)
            {
                return;
            }
            double nowTotalHours = this.Api.World.Calendar.TotalHours;
            bool weedIntervalOk = nowTotalHours - this.totalHoursLastWeed > cancrops.config.minHoursBetweenWeedStages;
            //wait few hours after weed stage was increased, also make a chance to replace sticks with normal grass block a little bit lower
            if (this.weedStage == WeedStage.HIGH && weedIntervalOk && rand.NextDouble() > 0.5)
            {
                double rnd = rand.NextDouble() * 3.45;
                int l = 0;

                foreach(var weed in cancrops.config.weedBlockCodes)
                {
                    rnd -= weed.Value;
                    if (rnd <= 0.0)
                    {
                        Block weedsBlock = this.Api.World.GetBlock(weed.Key);
                        if (weedsBlock != null)
                        {
                            this.Api.World.BlockAccessor.SetBlock(weedsBlock.BlockId, this.Pos);
                            return;
                        }
                        break;
                    }
                    else
                    {
                        l++;
                    }
                }
            }
            if (this.weedStage != WeedStage.HIGH && weedIntervalOk)
            {
                if (cancrops.config.weedChanceForCrossSticks + (int)this.weedStage * cancrops.config.weedStageBonusAdditionForSpawnChance > rand.NextDouble())
                {
                    this.weedStage += 1;
                    this.totalHoursLastWeed = this.Api.World.Calendar.TotalHours;
                    this.MarkDirty(true);
                    return;
                }
            }
            if (this.weedStage == WeedStage.HIGH)
            {
                BlockPos tmpPos;
                foreach (var dir in BlockFacing.HORIZONTALS)
                {
                    tmpPos = this.Pos.AddCopy(dir);
                    BlockEntity blockEntityFarmland = this.Api.World.BlockAccessor.GetBlockEntity(tmpPos);
                    if (cancrops.sapi.World.BlockAccessor.GetBlockEntity(this.Pos.AddCopy(dir)) is CANBECrop cbc)
                    {
                        cbc.TryPropagateWeed(this.weedStage, this, depth + 1);
                    }
                    else if (cancrops.sapi.World.BlockAccessor.GetBlockEntity(this.Pos.AddCopy(dir)) is CANBECrossSticks cbcs)
                    {
                        cbcs.TryPropagateWeed(this.weedStage, depth + 1);
                    }
                }
            }
        }
        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            if (((byItemStack != null) ? byItemStack.Attributes : null) != null)
            {
                string nowType = byItemStack.Attributes.GetString("type", "oak");
                if (nowType != this.type)
                {
                    this.type = nowType;
                    this.totalHoursLastWeed = Math.Max(0, this.Api.World.Calendar.TotalHours - cancrops.config.minHoursBetweenWeedStages);
                    this.MarkDirty(false, null);
                }
            }
            base.OnBlockPlaced(byItemStack);
        }
        private void loadOrCreateMesh()
        {
            this.ownMesh?.Dispose();
            CANBlockSelectionSticks block = base.Block as CANBlockSelectionSticks;
            if (base.Block == null)
            {
                block = (this.Api.World.BlockAccessor.GetBlock(this.Pos) as CANBlockSelectionSticks);
                base.Block = block;
            }
            if (block == null)
            {
                return;
            }
            string cacheKey = "crateMeshes" + block.FirstCodePart(0);
            Dictionary<string, MeshData> meshes = ObjectCacheUtil.GetOrCreate<Dictionary<string, MeshData>>(this.Api, cacheKey, () => new Dictionary<string, MeshData>());
            meshes.Clear();
            CompositeShape cshape = block.Shape.Clone();
            if (((cshape != null) ? cshape.Base : null) == null)
            {
                return;
            }
            MeshData weedMesh = null;
            if (weedStage != WeedStage.NONE)
            {
                Shape weedShape = null;

                if (weedStage == WeedStage.LOW)
                {
                    weedShape = Api.Assets.TryGet("cancrops:shapes/weed-1.json").ToObject<Shape>();
                }
                else if (weedStage == WeedStage.MEDIUM)
                {
                    weedShape = Api.Assets.TryGet("cancrops:shapes/weed-2.json").ToObject<Shape>();
                }
                else if (weedStage == WeedStage.HIGH)
                {
                    weedShape = Api.Assets.TryGet("cancrops:shapes/weed-3.json").ToObject<Shape>();
                }

                if (weedShape != null)
                {
                    (Api as ICoreClientAPI).Tesselator.TesselateShape(this.Block, weedShape, out weedMesh);
                    weedMesh.Translate(new Vintagestory.API.MathTools.Vec3f(0, 0f, 0));
                }
            }
            string meshKey = string.Concat(new string[]
            {
                            this.type, this.weedStage.ToString()
            });
            MeshData mesh;
            if (!meshes.TryGetValue(meshKey, out mesh))
            {
                mesh = block.GenMesh(this.Api as ICoreClientAPI, this.type, cshape, new Vec3f(cshape.rotateX, cshape.rotateY, cshape.rotateZ));
                if(weedMesh != null)
                {
                    mesh.AddMeshData(weedMesh);
                    //mesh = weedMesh;
                }
                meshes[meshKey] = mesh;
            }
            this.ownMesh = mesh.Clone();
        }
        public void OnCultivating(ItemSlot slot, EntityAgent byEntity)
        {
            if (!slot.Empty && slot.Itemstack.Item is CANItemHandCultivator)
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
        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            bool regenerateMesh = false;
            WeedStage newWeedStage = (WeedStage)tree.GetInt("weedStage", 0);
            if (this.weedStage != newWeedStage)
            {
                regenerateMesh = true;
                this.weedStage = newWeedStage;
            }
            this.type = tree.GetString("type", "oak");

            if (this.Api != null && this.Api.Side == EnumAppSide.Client)
            {
                this.loadOrCreateMesh();
                this.MarkDirty(true, null);
            }
            if (worldForResolving.Side == EnumAppSide.Client && this.capi != null)
            {
                if (regenerateMesh)
                {
                    loadOrCreateMesh();
                    //this.ownMesh = GenRightMesh();
                }
            }
            this.totalHoursLastWeed = tree.GetDouble("totalHoursLastWeed", 0.0);
            this.weedProtectionTicks = tree.GetInt("weedProtectionTicks", 0);
        }
        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetInt("weedStage", (int)this.weedStage);
            if (this.type == null)
            {
                this.type = "oak";
            }
            tree.SetString("type", this.type);
            tree.SetDouble("totalHoursLastWeed", this.totalHoursLastWeed);
            tree.SetInt("weedProtectionTicks", this.weedProtectionTicks);
        }
        public override void GetBlockInfo(IPlayer forPlayer, System.Text.StringBuilder sb)
        {
            base.GetBlockInfo(forPlayer, sb);
            if (this.weedProtectionTicks > 0)
            {
                sb.AppendLine(Lang.Get("cancrops:antiweed-ticks-left", this.weedProtectionTicks));
            }
        }
    }
}
