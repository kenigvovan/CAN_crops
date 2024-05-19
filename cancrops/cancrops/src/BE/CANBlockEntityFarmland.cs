using cancrops.src.blocks;
using cancrops.src.genetics;
using cancrops.src.items;
using cancrops.src.templates;
using cancrops.src.utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;

namespace cancrops.src.blockenities
{
    public class CANBlockEntityFarmland : BlockEntity, IFarmlandBlockEntity, IAnimalFoodSource, IPointOfInterest, ITexPositionSource
    {
        /*bool IsSuitableFor(Entity entity, CreatureDiet diet);

    float ConsumeOnePortion(Entity entity);*/
        private string[] creatureFoodTags;

        public override void Initialize(ICoreAPI api)
        {          
            base.Initialize(api);
            this.blockFarmland = (base.Block as CANBlockFarmland);
            if (this.blockFarmland == null)
            {
                return;
            }
            if(api.Side == EnumAppSide.Client)
            this.capi = (api as ICoreClientAPI);
            this.totalHoursWaterRetention = (double)this.Api.World.Calendar.HoursPerDay + 0.5;
            this.upPos = this.Pos.UpCopy(1);
            this.allowundergroundfarming = this.Api.World.Config.GetBool("allowUndergroundFarming", false);
            this.allowcropDeath = this.Api.World.Config.GetBool("allowCropDeath", true);
            this.fertilityRecoverySpeed = this.Api.World.Config.GetFloat("fertilityRecoverySpeed", this.fertilityRecoverySpeed);
            this.growthRateMul = (float)this.Api.World.Config.GetDecimal("cropGrowthRateMul", (double)this.growthRateMul);
            this.creatureFoodTags = base.Block.Attributes["foodTags"].AsArray<string>(null, null);
            if (api is ICoreServerAPI)
            {
                if (this.Api.World.Config.GetBool("processCrops", true))
                {
                    this.RegisterGameTickListener(new Action<float>(this.Update), 20000 + CANBlockEntityFarmland.rand.Next(400), 0);
                }
                api.ModLoader.GetModSystem<POIRegistry>(true).AddPOI(this);

                //init be farmlands around
                //only need when genome is set to be able to cross breed
                //ReadNeighbours();
            }
            else
            {
                if (this.currentRightMesh == null)
                {
                    this.currentRightMesh = this.GenRightMesh();
                    this.MarkDirty(true);
                }
            }
            this.updateFertilizerQuad();
        }

        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(byItemStack);
            if (Api.Side == EnumAppSide.Client)
            {
                currentRightMesh = GenRightMesh();
                MarkDirty(true);
            }
        }

        public void OnCreatedFromSoil(Block block)
        {
            string fertility = block.LastCodePart(1);
            if (block is BlockFarmland)
            {
                fertility = block.LastCodePart(0);
            }
            this.originalFertility[0] = (int)CANBlockEntityFarmland.Fertilities[fertility];
            this.originalFertility[1] = (int)CANBlockEntityFarmland.Fertilities[fertility];
            this.originalFertility[2] = (int)CANBlockEntityFarmland.Fertilities[fertility];
            this.nutrients[0] = (float)this.originalFertility[0];
            this.nutrients[1] = (float)this.originalFertility[1];
            this.nutrients[2] = (float)this.originalFertility[2];
            this.totalHoursLastUpdate = this.Api.World.Calendar.TotalHours;
            this.tryUpdateMoistureLevel(this.Api.World.Calendar.TotalDays, true);
        }

        public bool OnBlockInteract(IPlayer byPlayer)
        {
            ItemStack stack = byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack;
            JsonObject jsonObject;
            if (stack == null)
            {
                jsonObject = null;
            }
            else
            {
                CollectibleObject collectible = stack.Collectible;
                if (collectible == null)
                {
                    jsonObject = null;
                }
                else
                {
                    JsonObject attributes = collectible.Attributes;
                    jsonObject = ((attributes != null) ? attributes["fertilizerProps"] : null);
                }
            }
            JsonObject obj = jsonObject;
            if (obj == null || !obj.Exists)
            {
                return false;
            }
            FertilizerProps props = obj.AsObject<FertilizerProps>(null);
            if (props == null)
            {
                return false;
            }
            float nAdd = Math.Min(Math.Max(0f, 150f - this.slowReleaseNutrients[0]), props.N);
            float pAdd = Math.Min(Math.Max(0f, 150f - this.slowReleaseNutrients[1]), props.P);
            float kAdd = Math.Min(Math.Max(0f, 150f - this.slowReleaseNutrients[2]), props.K);
            this.slowReleaseNutrients[0] += nAdd;
            this.slowReleaseNutrients[1] += pAdd;
            this.slowReleaseNutrients[2] += kAdd;
            if (props.PermaBoost != null && !this.PermaBoosts.Contains(props.PermaBoost.Code))
            {
                this.originalFertility[0] += props.PermaBoost.N;
                this.originalFertility[1] += props.PermaBoost.P;
                this.originalFertility[2] += props.PermaBoost.K;
                this.PermaBoosts.Add(props.PermaBoost.Code);
            }
            string fertCode = stack.Collectible.Attributes["fertilizerTextureCode"].AsString(null);
            if (fertCode != null)
            {
                if (this.fertilizerOverlayStrength == null)
                {
                    this.fertilizerOverlayStrength = new Dictionary<string, float>();
                }
                float prevValue;
                this.fertilizerOverlayStrength.TryGetValue(fertCode, out prevValue);
                this.fertilizerOverlayStrength[fertCode] = prevValue + Math.Max(nAdd, Math.Max(kAdd, pAdd));
            }
            this.updateFertilizerQuad();
            byPlayer.InventoryManager.ActiveHotbarSlot.TakeOut(1);
            byPlayer.InventoryManager.ActiveHotbarSlot.MarkDirty();
            IClientPlayer clientPlayer = byPlayer as IClientPlayer;
            if (clientPlayer != null)
            {
                clientPlayer.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
            }
            this.Api.World.PlaySoundAt(this.Api.World.BlockAccessor.GetBlock(this.Pos).Sounds.Hit, (double)this.Pos.X + 0.5, (double)this.Pos.Y + 0.75, (double)this.Pos.Z + 0.5, byPlayer, true, 12f, 1f);
            this.MarkDirty(false, null);
            return true;
        }

        public void OnCropBlockBroken(IPlayer byPlayer)
        {
            this.ripeCropColdDamaged = false;
            this.unripeCropColdDamaged = false;
            this.unripeHeatDamaged = false;
            this.Genome = null;
            this.agriPlant = null;
            for (int i = 0; i < this.damageAccum.Length; i++)
            {
                this.damageAccum[i] = 0f;
            }
            /*if (this.Api.Side == EnumAppSide.Server)
            {
                BlockPos tmpPos;
                foreach (var dir in BlockFacing.HORIZONTALS)
                {
                    tmpPos = this.Pos.AddCopy(dir);
                    BlockEntity blockEntityFarmland = this.Api.World.BlockAccessor.GetBlockEntity(tmpPos);
                    if (cancrops.sapi.World.BlockAccessor.GetBlockEntity(this.Pos.AddCopy(dir)) is CANBlockEntityFarmland befl)
                    {
                        befl.OnNeighbourBroken(dir.Opposite);
                    }
                }
            }*/
            this.MarkDirty(true, null);
        }

        public List<ItemStack> RemoveDefaultSeeds(ItemStack[] drops)
        {

            List<ItemStack> li = new List<ItemStack>();
            foreach (var it in drops)
            {
                if (!(it.Item is ItemPlantableSeed))
                {
                    li.Add(it);
                }
            }
            return li;
        }

        public void ApplyStrengthBuff(List<ItemStack> drops)
        {
            var f = 3;
            f = 3;
            foreach(var it in drops)
            {
                float[] freshHours;
                float[] transitionHours;
                float[] transitionedHours;
                TransitionableProperties[] propsm = it.Collectible.GetTransitionableProperties(Api.World, it, null);
                ITreeAttribute attr = new TreeAttribute();
                if (propsm != null)
                    if (!it.Attributes.HasAttribute("createdTotalHours"))
                    {
                        attr.SetDouble("createdTotalHours", this.Api.World.Calendar.TotalHours);
                        attr.SetDouble("lastUpdatedTotalHours", Api.World.Calendar.TotalHours);
                        freshHours = new float[propsm.Length];
                        transitionHours = new float[propsm.Length];
                        transitionedHours = new float[propsm.Length];
                        for (int i = 0; i < propsm.Length; i++)
                        {
                            transitionedHours[i] = 0f;
                            freshHours[i] = propsm[i].FreshHours.nextFloat(1f, this.Api.World.Rand) * (1 + cancrops.config.strengthFreshHoursPercentBonus);
                            transitionHours[i] = propsm[i].TransitionHours.nextFloat(1f, this.Api.World.Rand);
                        }
                        attr["freshHours"] = new FloatArrayAttribute(freshHours);
                        attr["transitionHours"] = new FloatArrayAttribute(transitionHours);
                        attr["transitionedHours"] = new FloatArrayAttribute(transitionedHours);
                        it.Attributes["transitionstate"] = attr;
                    }
            }
        }
        public ItemStack[] GetDrops(ItemStack[] drops, IPlayer byPlayer)
        {
            if(this.upPos == null)
            {
                return drops;
            }
            List<ItemStack> newDrops = RemoveDefaultSeeds(drops);
            BlockEntityDeadCrop beDeadCrop = this.Api.World.BlockAccessor.GetBlockEntity(this.upPos) as BlockEntityDeadCrop;

            if (rand.NextDouble() < (agriPlant.SeedDropChance + agriPlant.SeedDropBonus /** GetCropStage(this.Block)*/))
            {
                var seed = CommonUtils.GetSeedItemStackFromFarmland(this.Genome, agriPlant);
                newDrops.Add(seed);
            }

            //if(Genome != null)
            //{
            int gain = Genome.Gain.Dominant.Value;
            foreach (var it in newDrops)
            {
                if(it.Item is ItemPlantableSeed)
                {
                    Block block = this.GetCrop();
                    int stage = 0;
                    if(block != null)
                    {
                        stage = this.GetCropStage(block);
                    }
                    
                    it.StackSize = Math.Min(2, (int)(agriPlant.SeedDropChance + agriPlant.SeedDropBonus * stage));
                    continue;
                        //(int)((gain * rand.Next(1, 3) * 0.2) * it.StackSize);
                }
                it.StackSize += (int)((gain * rand.Next(1, 3) * 0.2) * it.StackSize);                 
            }
            //}

            bool isDead = beDeadCrop != null;
            if (!this.ripeCropColdDamaged && !this.unripeCropColdDamaged && !this.unripeHeatDamaged && !isDead)
            {
                ApplyStrengthBuff(newDrops);
                return newDrops.ToArray();
            }
            if (!this.Api.World.Config.GetString("harshWinters", null).ToBool(true))
            {
                ApplyStrengthBuff(newDrops);
                return newDrops.ToArray();
            }
            List<ItemStack> stacks = new List<ItemStack>();
            Block crop = this.GetCrop();
            BlockCropProperties cropProps = (crop != null) ? crop.CropProps : null;
            if (cropProps == null)
            {
                return drops;
            }
            float mul = 1f;
            if (this.ripeCropColdDamaged)
            {
                mul = cropProps.ColdDamageRipeMul;
            }
            if (this.unripeHeatDamaged || this.unripeCropColdDamaged)
            {
                mul = cropProps.DamageGrowthStuntMul;
            }
            if (isDead)
            {
                mul = ((beDeadCrop.deathReason == EnumCropStressType.Eaten) ? 0f : Math.Max(cropProps.ColdDamageRipeMul, cropProps.DamageGrowthStuntMul));
            }
            foreach (ItemStack stack in newDrops)
            {
                if (stack.Collectible.NutritionProps == null)
                {
                    stacks.Add(stack);
                }
                else
                {
                    float q = (float)stack.StackSize * mul;
                    float frac = q - (float)((int)q);
                    stack.StackSize = (int)q + ((this.Api.World.Rand.NextDouble() > (double)frac) ? 1 : 0);
                    if (stack.StackSize > 0)
                    {
                        stacks.Add(stack);
                    }
                }
            }
            this.MarkDirty(true, null);
            return stacks.ToArray();
        }

        protected float GetNearbyWaterDistance(out CANBlockEntityFarmland.EnumWaterSearchResult result, float hoursPassed)
        {
            float waterDistance = 99f;
            this.farmlandIsAtChunkEdge = false;
            bool saltWater = false;
            this.Api.World.BlockAccessor.SearchFluidBlocks(new BlockPos(this.Pos.X - 4, this.Pos.Y, this.Pos.Z - 4), new BlockPos(this.Pos.X + 4, this.Pos.Y, this.Pos.Z + 4), delegate (Block block, BlockPos pos)
            {
                if (block.LiquidCode == "water")
                {
                    waterDistance = Math.Min(waterDistance, (float)Math.Max(Math.Abs(pos.X - this.Pos.X), Math.Abs(pos.Z - this.Pos.Z)));
                }
                if (block.LiquidCode == "saltwater")
                {
                    saltWater = true;
                }
                return true;
            }, delegate (int cx, int cy, int cz)
            {
                this.farmlandIsAtChunkEdge = true;
            });
            if (saltWater)
            {
                this.damageAccum[4] += hoursPassed;
            }
            result = CANBlockEntityFarmland.EnumWaterSearchResult.Deferred;
            if (this.farmlandIsAtChunkEdge)
            {
                return 99f;
            }
            this.lastWaterSearchedTotalHours = this.Api.World.Calendar.TotalHours;
            if (waterDistance < 4f)
            {
                result = CANBlockEntityFarmland.EnumWaterSearchResult.Found;
                return waterDistance;
            }
            result = CANBlockEntityFarmland.EnumWaterSearchResult.NotFound;
            return 99f;
        }

        private bool tryUpdateMoistureLevel(double totalDays, bool searchNearbyWater)
        {
            float dist = 99f;
            if (searchNearbyWater)
            {
                CANBlockEntityFarmland.EnumWaterSearchResult res;
                dist = this.GetNearbyWaterDistance(out res, 0f);
                if (res == CANBlockEntityFarmland.EnumWaterSearchResult.Deferred)
                {
                    return false;
                }
                if (res != CANBlockEntityFarmland.EnumWaterSearchResult.Found)
                {
                    dist = 99f;
                }
                this.lastWaterDistance = dist;
            }
            if (this.updateMoistureLevel(totalDays, dist))
            {
                this.UpdateFarmlandBlock();
            }
            return true;
        }

        private bool updateMoistureLevel(double totalDays, float waterDistance)
        {
            bool skyExposed = this.Api.World.BlockAccessor.GetRainMapHeightAt(this.Pos.X, this.Pos.Z) <= ((this.GetCrop() == null) ? this.Pos.Y : (this.Pos.Y + 1));
            return this.updateMoistureLevel(totalDays, waterDistance, skyExposed, null);
        }

        private bool updateMoistureLevel(double totalDays, float waterDistance, bool skyExposed, ClimateCondition baseClimate = null)
        {
            this.tmpPos.Set((double)this.Pos.X + 0.5, (double)this.Pos.Y + 0.5, (double)this.Pos.Z + 0.5);
            double hoursPassed = Math.Min((totalDays - this.lastMoistureLevelUpdateTotalDays) * (double)this.Api.World.Calendar.HoursPerDay, 48.0);
            if (hoursPassed < 0.029999999329447746)
            {
                this.moistureLevel = Math.Max(this.moistureLevel, GameMath.Clamp(1f - waterDistance / 4f, 0f, 1f));
                return false;
            }
            this.moistureLevel = Math.Max(0f, this.moistureLevel - (float)hoursPassed / 48f);
            this.moistureLevel = Math.Max(this.moistureLevel, GameMath.Clamp(1f - waterDistance / 4f, 0f, 1f));
            if (skyExposed)
            {
                if (baseClimate == null && hoursPassed > 0.0)
                {
                    baseClimate = this.Api.World.BlockAccessor.GetClimateAt(this.Pos, EnumGetClimateMode.WorldGenValues, totalDays - hoursPassed * (double)this.Api.World.Calendar.HoursPerDay / 2.0);
                }
                while (hoursPassed > 0.0)
                {
                    double rainLevel = (double)this.blockFarmland.wsys.GetPrecipitation(this.Pos, totalDays - hoursPassed * (double)this.Api.World.Calendar.HoursPerDay, baseClimate);
                    this.moistureLevel = GameMath.Clamp(this.moistureLevel + (float)rainLevel / 3f, 0f, 1f);
                    hoursPassed -= 1.0;
                }
            }
            this.lastMoistureLevelUpdateTotalDays = totalDays;
            return true;
        }

        private void Update(float dt)
        {
            if (!(this.Api as ICoreServerAPI).World.IsFullyLoadedChunk(this.Pos))
            {
                return;
            }
            double hoursNextStage = this.GetHoursForNextStage();
            bool nearbyWaterTested = false;
            double nowTotalHours = this.Api.World.Calendar.TotalHours;
            double hourIntervall = 3.0 + CANBlockEntityFarmland.rand.NextDouble();
            Block cropBlock = this.GetCrop();
            bool hasCrop = cropBlock != null;

            //LIGHT
            bool skyExposed = this.Api.World.BlockAccessor.GetRainMapHeightAt(this.Pos.X, this.Pos.Z) <= (hasCrop ? (this.Pos.Y + 1) : this.Pos.Y);
            if (nowTotalHours - this.totalHoursLastUpdate < hourIntervall)
            {
                if (this.updateMoistureLevel(this.Api.World.Calendar.TotalDays, this.lastWaterDistance, skyExposed, null))
                {
                    this.UpdateFarmlandBlock();
                }
                return;
            }
            int lightpenalty = 0;
            if (!this.allowundergroundfarming)
            {
                lightpenalty = Math.Max(0, this.Api.World.SeaLevel - this.Pos.Y);
            }
            int sunlight = this.Api.World.BlockAccessor.GetLightLevel(this.upPos, EnumLightLevelType.MaxLight);
            double lightGrowthSpeedFactor = (double)GameMath.Clamp(1f - (float)(this.blockFarmland.DelayGrowthBelowSunLight - sunlight - lightpenalty) * this.blockFarmland.LossPerLevel, 0f, 1f);
            
            
            Block upblock = this.Api.World.BlockAccessor.GetBlock(this.upPos);
            Block deadCropBlock = this.Api.World.GetBlock(new AssetLocation("deadcrop"));
            double lightHoursPenalty = hoursNextStage / lightGrowthSpeedFactor - hoursNextStage;
            double totalHoursNextGrowthState = this.totalHoursForNextStage + lightHoursPenalty;
            EnumSoilNutrient? currentlyConsumedNutrient = null;
            if (upblock.CropProps != null)
            {
                currentlyConsumedNutrient = new EnumSoilNutrient?(upblock.CropProps.RequiredNutrient);
            }
            bool growTallGrass = false;
            float[] npkRegain = new float[3];
            float waterDistance = 99f;
            this.totalHoursLastUpdate = Math.Max(this.totalHoursLastUpdate, nowTotalHours - (double)((float)this.Api.World.Calendar.DaysPerYear * this.Api.World.Calendar.HoursPerDay));
            bool hasRipeCrop = this.HasRipeCrop();
            if (!skyExposed)
            {
                RoomRegistry roomreg = this.blockFarmland.roomreg;
                Room room = (roomreg != null) ? roomreg.GetRoomForPosition(this.upPos) : null;
                this.roomness = ((room != null && room.SkylightCount > room.NonSkylightCount && room.ExitCount == 0) ? 1 : 0);
            }
            else
            {
                this.roomness = 0;
            }
            ClimateCondition baseClimate = this.Api.World.BlockAccessor.GetClimateAt(this.Pos, EnumGetClimateMode.WorldGenValues, 0.0);
            if (baseClimate == null)
            {
                return;
            }
            float baseTemperature = baseClimate.Temperature;
            while (nowTotalHours - this.totalHoursLastUpdate > hourIntervall)
            {
                if (!nearbyWaterTested)
                {
                    CANBlockEntityFarmland.EnumWaterSearchResult res;
                    waterDistance = this.GetNearbyWaterDistance(out res, (float)hourIntervall);
                    if (res == CANBlockEntityFarmland.EnumWaterSearchResult.Deferred)
                    {
                        return;
                    }
                    if (res == CANBlockEntityFarmland.EnumWaterSearchResult.NotFound)
                    {
                        waterDistance = 99f;
                    }
                    nearbyWaterTested = true;
                    this.lastWaterDistance = waterDistance;
                }
                this.updateMoistureLevel(this.totalHoursLastUpdate / (double)this.Api.World.Calendar.HoursPerDay, waterDistance, skyExposed, baseClimate);
                this.totalHoursLastUpdate += hourIntervall;
                hourIntervall = 3.0 + CANBlockEntityFarmland.rand.NextDouble();
                baseClimate.Temperature = baseTemperature;
                ClimateCondition conds = this.Api.World.BlockAccessor.GetClimateAt(this.Pos, baseClimate, EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, this.totalHoursLastUpdate / (double)this.Api.World.Calendar.HoursPerDay);
                
                
                //TEMPERATURE
                if (this.roomness > 0)
                {
                    conds.Temperature += 5f;
                }
                if (!hasCrop)
                {
                    this.ripeCropColdDamaged = false;
                    this.unripeCropColdDamaged = false;
                    this.unripeHeatDamaged = false;
                    for (int i = 0; i < this.damageAccum.Length; i++)
                    {
                        this.damageAccum[i] = 0f;
                    }
                }
                else
                {
                    float tempBuff = 0;
                    if (Genome != null)
                    {
                        tempBuff = cancrops.config.coldResistanceByStat * Genome.Resistance.Dominant.Value;
                    }
                    if (((cropBlock != null) ? cropBlock.CropProps : null) != null && conds.Temperature < (cropBlock.CropProps.ColdDamageBelow - tempBuff))
                    {
                        if (hasRipeCrop)
                        {
                            this.ripeCropColdDamaged = true;
                        }
                        else
                        {
                            this.unripeCropColdDamaged = true;
                            this.damageAccum[2] += (float)hourIntervall;
                        }
                    }
                    else
                    {
                        this.damageAccum[2] = Math.Max(0f, this.damageAccum[2] - (float)hourIntervall / 10f);
                    }

                    if (Genome != null)
                    {
                        tempBuff = cancrops.config.heatResistanceByStat * Genome.Resistance.Dominant.Value;
                    }
                    else
                    {
                        tempBuff = 0;
                    }

                    if (((cropBlock != null) ? cropBlock.CropProps : null) != null && conds.Temperature > (cropBlock.CropProps.HeatDamageAbove + tempBuff) && hasCrop)
                    {
                        this.unripeHeatDamaged = true;
                        this.damageAccum[1] += (float)hourIntervall;
                    }
                    else
                    {
                        this.damageAccum[1] = Math.Max(0f, this.damageAccum[1] - (float)hourIntervall / 10f);
                    }
                    for (int j = 0; j < this.damageAccum.Length; j++)
                    {
                        float dmg = this.damageAccum[j];
                        if (!this.allowcropDeath)
                        {
                            dmg = (this.damageAccum[j] = 0f);
                        }
                        if (dmg > 48f)
                        {
                            this.Api.World.BlockAccessor.SetBlock(deadCropBlock.Id, this.upPos);
                            BlockEntityDeadCrop blockEntityDeadCrop = this.Api.World.BlockAccessor.GetBlockEntity(this.upPos) as BlockEntityDeadCrop;
                            blockEntityDeadCrop.Inventory[0].Itemstack = new ItemStack(cropBlock, 1);
                            blockEntityDeadCrop.deathReason = (EnumCropStressType)j;
                            hasCrop = false;
                            break;
                        }
                    }
                }
                float growthChance = GameMath.Clamp(conds.Temperature / 10f, 0f, 10f);
                if (CANBlockEntityFarmland.rand.NextDouble() <= (double)growthChance)
                {
                    growTallGrass |= (CANBlockEntityFarmland.rand.NextDouble() < 0.006);
                    bool ripe = this.HasRipeCrop();
                    npkRegain[0] = (ripe ? 0f : this.fertilityRecoverySpeed);
                    npkRegain[1] = (ripe ? 0f : this.fertilityRecoverySpeed);
                    npkRegain[2] = (ripe ? 0f : this.fertilityRecoverySpeed);
                    if (currentlyConsumedNutrient != null)
                    {
                        npkRegain[(int)currentlyConsumedNutrient.Value] /= 3f;
                    }
                    for (int k = 0; k < 3; k++)
                    {
                        this.nutrients[k] += Math.Max(0f, npkRegain[k] + Math.Min(0f, (float)this.originalFertility[k] - this.nutrients[k] - npkRegain[k]));
                        if (this.slowReleaseNutrients[k] > 0f)
                        {
                            float release = Math.Min(0.25f, this.slowReleaseNutrients[k]);
                            this.nutrients[k] = Math.Min(100f, this.nutrients[k] + release);
                            this.slowReleaseNutrients[k] = Math.Max(0f, this.slowReleaseNutrients[k] - release);
                        }
                        else if (this.nutrients[k] > (float)this.originalFertility[k])
                        {
                            this.nutrients[k] = Math.Max((float)this.originalFertility[k], this.nutrients[k] - 0.05f);
                        }
                    }
                    if (this.fertilizerOverlayStrength != null && this.fertilizerOverlayStrength.Count > 0)
                    {
                        foreach (string code in this.fertilizerOverlayStrength.Keys.ToArray<string>())
                        {
                            float newStr = this.fertilizerOverlayStrength[code] - this.fertilityRecoverySpeed;
                            if (newStr < 0f)
                            {
                                this.fertilizerOverlayStrength.Remove(code);
                            }
                            else
                            {
                                this.fertilizerOverlayStrength[code] = newStr;
                            }
                        }
                    }
                    if ((double)this.moistureLevel >= 0.1 && totalHoursNextGrowthState <= this.totalHoursLastUpdate)
                    {
                        this.TryGrowCrop(this.totalHoursForNextStage);
                        this.totalHoursForNextStage += hoursNextStage;
                        totalHoursNextGrowthState = this.totalHoursForNextStage + lightHoursPenalty;
                        hoursNextStage = this.GetHoursForNextStage();
                    }
                }
            }
            if (growTallGrass && upblock.BlockMaterial == EnumBlockMaterial.Air)
            {
                double rnd = CANBlockEntityFarmland.rand.NextDouble() * (double)this.blockFarmland.TotalWeedChance;
                int l = 0;
                while (l < this.blockFarmland.WeedNames.Length)
                {
                    rnd -= (double)this.blockFarmland.WeedNames[l].Chance;
                    if (rnd <= 0.0)
                    {
                        if(this.cropSticksVariant != EnumCropSticksVariant.NONE)
                        {
                            this.cropSticksVariant = EnumCropSticksVariant.NONE;
                            this.MarkDirty();
                        }
                        Block weedsBlock = this.Api.World.GetBlock(this.blockFarmland.WeedNames[l].Code);
                        if (weedsBlock != null)
                        {
                            this.Api.World.BlockAccessor.SetBlock(weedsBlock.BlockId, this.upPos);
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
            if(isCrossCrop())
            {
                executeCrossGrowthTick();
            }
            this.updateFertilizerQuad();
            this.UpdateFarmlandBlock();
            this.Api.World.BlockAccessor.MarkBlockEntityDirty(this.Pos);
        }

        public double GetHoursForNextStage()
        {
            Block block = this.GetCrop();
            if (block == null)
            {
                return 99999999.0;
            }
            float totalDays = block.CropProps.TotalGrowthDays;
            if (totalDays > 0f)
            {
                totalDays = totalDays / 12f * (float)this.Api.World.Calendar.DaysPerMonth;
            }
            else
            {
                totalDays = block.CropProps.TotalGrowthMonths * (float)this.Api.World.Calendar.DaysPerMonth;
            }
            if (Genome != null)
            {
                return (double)(this.Api.World.Calendar.HoursPerDay * totalDays 
                    / (float)block.CropProps.GrowthStages 
                    * (1f / this.GetGrowthRate(block.CropProps.RequiredNutrient))
                    * (float)(0.9 + 0.2 * CANBlockEntityFarmland.rand.NextDouble())
                    / this.growthRateMul)
                        * (1f - (Genome.Growth.Dominant.Value * 0.05));
            }
            else
            {
                return (double)(this.Api.World.Calendar.HoursPerDay * totalDays / (float)block.CropProps.GrowthStages * (1f / this.GetGrowthRate(block.CropProps.RequiredNutrient)) * (float)(0.9 + 0.2 * CANBlockEntityFarmland.rand.NextDouble()) / this.growthRateMul);
            }
        }

        public float GetGrowthRate(EnumSoilNutrient nutrient)
        {
            float moistFactor = (float)Math.Pow(Math.Max(0.01, (double)(this.moistureLevel * 100f / 70f) - 0.143), 0.35);
            if (this.nutrients[(int)nutrient] > 75f)
            {
                return moistFactor * 1.1f;
            }
            if (this.nutrients[(int)nutrient] > 50f)
            {
                return moistFactor * 1f;
            }
            if (this.nutrients[(int)nutrient] > 35f)
            {
                return moistFactor * 0.9f;
            }
            if (this.nutrients[(int)nutrient] > 20f)
            {
                return moistFactor * 0.6f;
            }
            if (this.nutrients[(int)nutrient] > 5f)
            {
                return moistFactor * 0.3f;
            }
            return moistFactor * 0.1f;
        }

        public float GetGrowthRate()
        {
            Block crop = this.GetCrop();
            BlockCropProperties cropProps = (crop != null) ? crop.CropProps : null;
            if (cropProps != null)
            {
                return this.GetGrowthRate(cropProps.RequiredNutrient);
            }
            return 1f;
        }

        public float GetDeathChance(int nutrientIndex)
        {
            if (this.nutrients[nutrientIndex] <= 5f)
            {
                return 0.5f;
            }
            return 0f;
        }

        public bool TryPlant(Block block, ItemStack itemStack, AgriPlant agriPlant)
        {
            if (this.CanPlant() && block.CropProps != null && !this.isCrossCrop())
            {
                //search for genome on seed (or get default one)
                //search for agriplant of the seed and if not then return false
                //otherwise set genome and plant
                if(Api.Side == EnumAppSide.Server)
                {
                    Genome seedGenome = CommonUtils.GetSeedGenomeFromAttribute(itemStack);
                    //AgriPlant agriPlant = cancrops.GetPlants().getPlant(itemStack.Item.Code.Domain + ":" + itemStack.Item.LastCodePart(0));

                    /*if (agriPlant == null) 
                    {
                        return false;
                    }*/
                    this.Genome = seedGenome;
                    this.agriPlant = agriPlant;
                }
                this.Api.World.BlockAccessor.SetBlock(block.BlockId, this.upPos);
                this.totalHoursForNextStage = this.Api.World.Calendar.TotalHours + this.GetHoursForNextStage();
                CropBehavior[] behaviors = block.CropProps.Behaviors;
                for (int i = 0; i < behaviors.Length; i++)
                {
                    behaviors[i].OnPlanted(this.Api);
                }
                ReadNeighbours();
                return true;
            }
            return false;
        }

        public bool CanPlant()
        {
            if(this.upPos == null)
            {
                return false;
            }
            Block block = this.Api.World.BlockAccessor.GetBlock(this.upPos);
            return block == null || block.BlockMaterial == EnumBlockMaterial.Air;
        }

        public bool HasUnripeCrop()
        {
            Block block = this.GetCrop();
            return block != null && this.GetCropStage(block) < block.CropProps.GrowthStages;
        }

        public bool HasRipeCrop()
        {
            Block block = this.GetCrop();
            return block != null && this.GetCropStage(block) >= block.CropProps.GrowthStages;
        }

        public bool TryGrowCrop(double currentTotalHours)
        {
            Block block = this.GetCrop();
            if (block == null)
            {
                return false;
            }
            int currentGrowthStage = this.GetCropStage(block);
            if (currentGrowthStage >= block.CropProps.GrowthStages)
            {
                return false;
            }
            int newGrowthStage = currentGrowthStage + 1;
            Block nextBlock = this.Api.World.GetBlock(block.CodeWithParts(newGrowthStage.ToString() ?? ""));
            if (nextBlock == null)
            {
                return false;
            }
            if (block.CropProps.Behaviors != null)
            {
                EnumHandling handled = EnumHandling.PassThrough;
                bool result = false;
                CropBehavior[] behaviors = block.CropProps.Behaviors;
                for (int i = 0; i < behaviors.Length; i++)
                {
                    result = behaviors[i].TryGrowCrop(this.Api, this, currentTotalHours, newGrowthStage, ref handled);
                    if (handled == EnumHandling.PreventSubsequent)
                    {
                        return result;
                    }
                }
                if (handled == EnumHandling.PreventDefault)
                {
                    return result;
                }
            }
            if (this.Api.World.BlockAccessor.GetBlockEntity(this.upPos) == null)
            {
                this.Api.World.BlockAccessor.SetBlock(nextBlock.BlockId, this.upPos);
            }
            else
            {
                this.Api.World.BlockAccessor.ExchangeBlock(nextBlock.BlockId, this.upPos);
            }
            this.ConsumeNutrients(block);
            return true;
        }

        private void ConsumeNutrients(Block cropBlock)
        {
            float nutrientLoss = cropBlock.CropProps.NutrientConsumption / (float)cropBlock.CropProps.GrowthStages;
            this.nutrients[(int)cropBlock.CropProps.RequiredNutrient] = Math.Max(0f, this.nutrients[(int)cropBlock.CropProps.RequiredNutrient] - nutrientLoss);
            this.UpdateFarmlandBlock();
        }
        

        private void UpdateFarmlandBlock()
        {
            int nowLevel = this.GetFertilityLevel((float)((this.originalFertility[0] + this.originalFertility[1] + this.originalFertility[2]) / 3));
            Block farmlandBlock = this.Api.World.BlockAccessor.GetBlock(this.Pos);
            Block nextFarmlandBlock = this.Api.World.GetBlock(farmlandBlock.CodeWithParts(new string[]
            {
                this.IsVisiblyMoist ? "moist" : "dry",
                CANBlockEntityFarmland.Fertilities.GetKeyAtIndex(nowLevel)
            }));
            if (nextFarmlandBlock == null)
            {
                this.Api.World.BlockAccessor.RemoveBlockEntity(this.Pos);
                return;
            }
            if (farmlandBlock.BlockId != nextFarmlandBlock.BlockId)
            {
                this.Api.World.BlockAccessor.ExchangeBlock(nextFarmlandBlock.BlockId, this.Pos);
                this.Api.World.BlockAccessor.MarkBlockEntityDirty(this.Pos);
                this.Api.World.BlockAccessor.MarkBlockDirty(this.Pos);
            }
        }

        internal int GetFertilityLevel(float fertiltyValue)
        {
            int i = 0;
            foreach (KeyValuePair<string, float> val in CANBlockEntityFarmland.Fertilities)
            {
                if (val.Value >= fertiltyValue)
                {
                    return i;
                }
                i++;
            }
            return CANBlockEntityFarmland.Fertilities.Count - 1;
        }

        internal Block GetCrop()
        {
            if(this.upPos == null)
            {
                return null;
            }
            Block block = this.Api.World.BlockAccessor.GetBlock(this.upPos);
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
        public int GetCropStageWithout()
        {
            Block crop = this.GetCrop();
            if (crop != null)
            {
                return this.GetCropStage(this.GetCrop());
            }
            return 0;
        }
        private void updateFertilizerQuad()
        {
            if (this.capi == null)
            {
                return;
            }
            AssetLocation loc = new AssetLocation();
            if (this.fertilizerOverlayStrength == null || this.fertilizerOverlayStrength.Count == 0)
            {
                bool flag = this.fertilizerQuad != null;
                this.fertilizerQuad = null;
                if (flag)
                {
                    this.MarkDirty(true, null);
                }
                return;
            }
            int i = 0;
            foreach (KeyValuePair<string, float> val in this.fertilizerOverlayStrength)
            {
                string intensity = "low";
                if (val.Value > 50f)
                {
                    intensity = "med";
                }
                if (val.Value > 100f)
                {
                    intensity = "high";
                }
                if (i > 0)
                {
                    AssetLocation assetLocation = loc;
                    assetLocation.Path += "++0~";
                }
                AssetLocation assetLocation2 = loc;
                assetLocation2.Path = string.Concat(new string[]
                {
                    assetLocation2.Path,
                    "block/soil/farmland/fertilizer/",
                    val.Key,
                    "-",
                    intensity
                });
                i++;
            }
            int num;
            TextureAtlasPosition newFertilizerTexturePos;
            this.capi.BlockTextureAtlas.GetOrInsertTexture(loc, out num, out newFertilizerTexturePos, null, 0f);
            if (this.fertilizerTexturePos != newFertilizerTexturePos)
            {
                this.fertilizerTexturePos = newFertilizerTexturePos;
                this.genFertilizerQuad();
                this.MarkDirty(true, null);
            }
        }

        private void genFertilizerQuad()
        {
            //var c = this.capi.BlockTextureAtlas.Size;
            Shape shape = this.capi.Assets.TryGet(new AssetLocation("game:shapes/block/farmland-fertilizer.json"), true).ToObject<Shape>(null);
            //var f = this.AtlasSize;
            this.capi.Tesselator.TesselateShape(new TesselationMetaData
            {
                TypeForLogging = "farmland fertilizer quad",
                TexSource = this
            }, shape, out this.fertilizerQuad);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            this.nutrients[0] = tree.GetFloat("n", 0f);
            this.nutrients[1] = tree.GetFloat("p", 0f);
            this.nutrients[2] = tree.GetFloat("k", 0f);
            this.slowReleaseNutrients[0] = tree.GetFloat("slowN", 0f);
            this.slowReleaseNutrients[1] = tree.GetFloat("slowP", 0f);
            this.slowReleaseNutrients[2] = tree.GetFloat("slowK", 0f);
            this.moistureLevel = tree.GetFloat("moistureLevel", 0f);
            this.lastWaterSearchedTotalHours = tree.GetDouble("lastWaterSearchedTotalHours", 0.0);
            if (!tree.HasAttribute("originalFertilityN"))
            {
                this.originalFertility[0] = tree.GetInt("originalFertility", 0);
                this.originalFertility[1] = tree.GetInt("originalFertility", 0);
                this.originalFertility[2] = tree.GetInt("originalFertility", 0);
            }
            else
            {
                this.originalFertility[0] = tree.GetInt("originalFertilityN", 0);
                this.originalFertility[1] = tree.GetInt("originalFertilityP", 0);
                this.originalFertility[2] = tree.GetInt("originalFertilityK", 0);
            }
            if (tree.HasAttribute("totalHoursForNextStage"))
            {
                this.totalHoursForNextStage = tree.GetDouble("totalHoursForNextStage", 0.0);
                this.totalHoursLastUpdate = tree.GetDouble("totalHoursFertilityCheck", 0.0);
            }
            else
            {
                this.totalHoursForNextStage = tree.GetDouble("totalDaysForNextStage", 0.0) * 24.0;
                this.totalHoursLastUpdate = tree.GetDouble("totalDaysFertilityCheck", 0.0) * 24.0;
            }
            this.lastMoistureLevelUpdateTotalDays = tree.GetDouble("lastMoistureLevelUpdateTotalDays", 0.0);
            this.cropAttrs = (tree["cropAttrs"] as TreeAttribute);
            if (this.cropAttrs == null)
            {
                this.cropAttrs = new TreeAttribute();
            }
            this.lastWaterDistance = tree.GetFloat("lastWaterDistance", 0f);
            this.unripeCropColdDamaged = tree.GetBool("unripeCropExposedToFrost", false);
            this.ripeCropColdDamaged = tree.GetBool("ripeCropExposedToFrost", false);
            this.unripeHeatDamaged = tree.GetBool("unripeHeatDamaged", false);
            this.saltExposed = tree.GetBool("saltExposed", false);
            this.roomness = tree.GetInt("roomness", 0);
            string[] permaboosts = (tree as TreeAttribute).GetStringArray("permaBoosts", null);
            if (permaboosts != null)
            {
                this.PermaBoosts.AddRange(permaboosts);
            }
            ITreeAttribute ftree = tree.GetTreeAttribute("fertilizerOverlayStrength");
            if (ftree != null)
            {
                this.fertilizerOverlayStrength = new Dictionary<string, float>();
                using (IEnumerator<KeyValuePair<string, IAttribute>> enumerator = ftree.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<string, IAttribute> val = enumerator.Current;
                        this.fertilizerOverlayStrength[val.Key] = (val.Value as FloatAttribute).value;
                    }
                    //goto IL_316;
                }
            }
            //this.fertilizerOverlayStrength = null;
            //cropSticksVariant
            var tmpStick = (EnumCropSticksVariant)tree.GetInt("cropSticksVariant");
            if (cropSticksVariant != tmpStick)
            {
                GenRightMesh();
                cropSticksVariant = tmpStick;
            }
        IL_316:
            this.updateFertilizerQuad();
            //if (this.Api.Side == EnumAppSide.Server)
            {
                if (tree.HasAttribute("genome"))
                {
                    this.Genome = Genome.FromTreeAttribute(tree.GetTreeAttribute("genome"));
                }

                if (tree.HasAttribute("plant"))
                {
                    this.agriPlant = cancrops.GetPlants()?.getPlant(tree.GetString("plant")) ?? null;
                }
            }
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetFloat("n", this.nutrients[0]);
            tree.SetFloat("p", this.nutrients[1]);
            tree.SetFloat("k", this.nutrients[2]);
            tree.SetFloat("slowN", this.slowReleaseNutrients[0]);
            tree.SetFloat("slowP", this.slowReleaseNutrients[1]);
            tree.SetFloat("slowK", this.slowReleaseNutrients[2]);
            tree.SetFloat("moistureLevel", this.moistureLevel);
            tree.SetDouble("lastWaterSearchedTotalHours", this.lastWaterSearchedTotalHours);
            tree.SetInt("originalFertilityN", this.originalFertility[0]);
            tree.SetInt("originalFertilityP", this.originalFertility[1]);
            tree.SetInt("originalFertilityK", this.originalFertility[2]);
            tree.SetDouble("totalHoursForNextStage", this.totalHoursForNextStage);
            tree.SetDouble("totalHoursFertilityCheck", this.totalHoursLastUpdate);
            tree.SetDouble("lastMoistureLevelUpdateTotalDays", this.lastMoistureLevelUpdateTotalDays);
            tree.SetFloat("lastWaterDistance", this.lastWaterDistance);
            tree.SetBool("ripeCropExposedToFrost", this.ripeCropColdDamaged);
            tree.SetBool("unripeCropExposedToFrost", this.unripeCropColdDamaged);
            tree.SetBool("unripeHeatDamaged", this.unripeHeatDamaged);
            tree.SetBool("saltExposed", this.damageAccum[4] > 1f);
            (tree as TreeAttribute).SetStringArray("permaBoosts", this.PermaBoosts.ToArray<string>());
            tree.SetInt("roomness", this.roomness);
            tree["cropAttrs"] = this.cropAttrs;
            if (this.fertilizerOverlayStrength != null)
            {
                TreeAttribute ftree = new TreeAttribute();
                tree["fertilizerOverlayStrength"] = ftree;
                foreach (KeyValuePair<string, float> val in this.fertilizerOverlayStrength)
                {
                    ftree.SetFloat(val.Key, val.Value);
                }
            }
            tree.SetInt("cropSticksVariant", (int)cropSticksVariant);
            //if (this.Api.Side == EnumAppSide.Server)
            {
                if (Genome != null)
                {
                    tree["genome"] = this.Genome.AsTreeAttribute();
                }
                if (agriPlant != null)
                {
                    tree.SetString("plant", this.agriPlant.Id);
                }
            }
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            Block crop = this.GetCrop();
            BlockCropProperties cropProps = (crop != null) ? crop.CropProps : null;
            if (cropProps != null)
            {
                dsc.AppendLine(Lang.Get("Required Nutrient: {0}", new object[]
                {
                    cropProps.RequiredNutrient
                }));
                dsc.AppendLine(Lang.Get("Growth Stage: {0} / {1}", new object[]
                {
                    this.GetCropStage(this.GetCrop()),
                    cropProps.GrowthStages
                }));
                dsc.AppendLine();
            }
            dsc.AppendLine(Lang.Get("farmland-nutrientlevels", new object[]
            {
                Math.Round((double)this.nutrients[0], 1),
                Math.Round((double)this.nutrients[1], 1),
                Math.Round((double)this.nutrients[2], 1)
            }));
            float snn = (float)Math.Round((double)this.slowReleaseNutrients[0], 1);
            float snp = (float)Math.Round((double)this.slowReleaseNutrients[1], 1);
            float snk = (float)Math.Round((double)this.slowReleaseNutrients[2], 1);
            if (snn > 0f || snp > 0f || snk > 0f)
            {
                List<string> nutrs = new List<string>();
                if (snn > 0f)
                {
                    nutrs.Add(Lang.Get("+{0}% N", new object[]
                    {
                        snn
                    }));
                }
                if (snp > 0f)
                {
                    nutrs.Add(Lang.Get("+{0}% P", new object[]
                    {
                        snp
                    }));
                }
                if (snk > 0f)
                {
                    nutrs.Add(Lang.Get("+{0}% K", new object[]
                    {
                        snk
                    }));
                }
                dsc.AppendLine(Lang.Get("farmland-activefertilizer", new object[]
                {
                    string.Join(", ", nutrs)
                }));
            }
            if (cropProps == null)
            {
                float speedn = (float)Math.Round((double)(100f * this.GetGrowthRate(EnumSoilNutrient.N)), 0);
                float speedp = (float)Math.Round((double)(100f * this.GetGrowthRate(EnumSoilNutrient.P)), 0);
                float speedk = (float)Math.Round((double)(100f * this.GetGrowthRate(EnumSoilNutrient.K)), 0);
                string colorn = ColorUtil.Int2Hex(GuiStyle.DamageColorGradient[(int)Math.Min(99f, speedn)]);
                string colorp = ColorUtil.Int2Hex(GuiStyle.DamageColorGradient[(int)Math.Min(99f, speedp)]);
                string colork = ColorUtil.Int2Hex(GuiStyle.DamageColorGradient[(int)Math.Min(99f, speedk)]);
                dsc.AppendLine(Lang.Get("farmland-growthspeeds", new object[]
                {
                    colorn,
                    speedn,
                    colorp,
                    speedp,
                    colork,
                    speedk
                }));
            }
            else
            {
                float speed = (float)Math.Round((double)(100f * this.GetGrowthRate(cropProps.RequiredNutrient)), 0);
                string color = ColorUtil.Int2Hex(GuiStyle.DamageColorGradient[(int)Math.Min(99f, speed)]);
                dsc.AppendLine(Lang.Get("farmland-growthspeed", new object[]
                {
                    color,
                    speed,
                    cropProps.RequiredNutrient
                }));
            }
            float moisture = (float)Math.Round((double)(this.moistureLevel * 100f), 0);
            string colorm = ColorUtil.Int2Hex(GuiStyle.DamageColorGradient[(int)Math.Min(99f, moisture)]);
            dsc.AppendLine(Lang.Get("farmland-moisture", new object[]
            {
                colorm,
                moisture
            }));
            if ((this.ripeCropColdDamaged || this.unripeCropColdDamaged || this.unripeHeatDamaged) && cropProps != null)
            {
                if (this.ripeCropColdDamaged)
                {
                    dsc.AppendLine(Lang.Get("farmland-ripecolddamaged", new object[]
                    {
                        (int)(cropProps.ColdDamageRipeMul * 100f)
                    }));
                }
                else if (this.unripeCropColdDamaged)
                {
                    dsc.AppendLine(Lang.Get("farmland-unripecolddamaged", new object[]
                    {
                        (int)(cropProps.DamageGrowthStuntMul * 100f)
                    }));
                }
                else if (this.unripeHeatDamaged)
                {
                    dsc.AppendLine(Lang.Get("farmland-unripeheatdamaged", new object[]
                    {
                        (int)(cropProps.DamageGrowthStuntMul * 100f)
                    }));
                }
            }
            if (this.roomness > 0)
            {
                dsc.AppendLine(Lang.Get("greenhousetempbonus", Array.Empty<object>()));
            }
            if (this.saltExposed)
            {
                dsc.AppendLine(Lang.Get("farmland-saltdamage", Array.Empty<object>()));
            }
            dsc.ToString();
        }

        public void WaterFarmland(float dt, bool waterNeightbours = true)
        {
            this.moistureLevel = Math.Min(1f, this.moistureLevel + dt / 2f);
            if (waterNeightbours)
            {
                foreach (BlockFacing neib in BlockFacing.HORIZONTALS)
                {
                    BlockPos npos = this.Pos.AddCopy(neib);
                    CANBlockEntityFarmland bef = this.Api.World.BlockAccessor.GetBlockEntity(npos) as CANBlockEntityFarmland;
                    if (bef != null)
                    {
                        bef.WaterFarmland(dt / 3f, false);
                    }
                }
            }
            this.updateMoistureLevel(this.Api.World.Calendar.TotalDays, this.lastWaterDistance);
            this.UpdateFarmlandBlock();
        }       
        public void ReadNeighbours()
        {
            neighbours.Clear();
            foreach (var dir in BlockFacing.HORIZONTALS)
            {
                //var f = this.Pos.AddCopy(dir);
                if (cancrops.sapi.World.BlockAccessor.GetBlockEntity(this.Pos.AddCopy(dir)) is CANBlockEntityFarmland befl)
                {
                    if (befl.hasPlant())
                    {
                        neighbours[dir] = befl;
                    }
                }
                else
                {
                    neighbours.Remove(dir);
                }
            }
        }
        public bool isCrossCrop()
        {
            return this.cropSticksVariant == EnumCropSticksVariant.DOUBLE;
        }
        public EnumCropSticksVariant GetCropSticksVariant()
        {
            return cropSticksVariant;
        }
        public bool TryPlaceSelectionSticks()
        {
            if(Genome != null)
            {
                return false;
            }
            if(cropSticksVariant < EnumCropSticksVariant.DOUBLE)
            {
                //var f = cropSticksVariant+1;
                cropSticksVariant = (EnumCropSticksVariant)(cropSticksVariant+1);
                this.MarkDirty(true);
                if (cropSticksVariant == EnumCropSticksVariant.DOUBLE)
                {
                    ReadNeighbours();
                }
                return true;
            }
            return false;
        }
        protected void executeCrossGrowthTick()
        {
            // Do not do mutation growth ticks if the plant has weeds
            //if (!this.hasWeeds())
            {
                if (cancrops.GetAgriMutationHandler().handleCrossBreedTick(this, this.neighbours.Values, rand))
                {
                    //MinecraftForge.EVENT_BUS.post(new AgriCropEvent.Grow.Cross.Post(this));
                }
            }
        }
        private Dictionary<BlockFacing, CANBlockEntityFarmland> neighbours =new Dictionary<BlockFacing, CANBlockEntityFarmland>();
        private EnumCropSticksVariant cropSticksVariant = EnumCropSticksVariant.NONE;

        public bool IsSuitableFor(Entity entity, CreatureDiet diet)
        {
            if (diet == null)
            {
                return false;
            }
            Block crop = this.GetCrop();
            if (crop == null)
            {
                return false;
            }
            return diet.Matches(EnumFoodCategory.NoNutrition, this.creatureFoodTags);
        }


        public float ConsumeOnePortion(Entity entity)
        {
            Block crop = this.GetCrop();
            if (crop == null)
            {
                return 0f;
            }
            Block block = this.Api.World.GetBlock(new AssetLocation("deadcrop"));
            this.Api.World.BlockAccessor.SetBlock(block.Id, this.upPos);
            BlockEntityDeadCrop blockEntityDeadCrop = this.Api.World.BlockAccessor.GetBlockEntity(this.upPos) as BlockEntityDeadCrop;
            blockEntityDeadCrop.Inventory[0].Itemstack = new ItemStack(crop, 1);
            blockEntityDeadCrop.deathReason = EnumCropStressType.Eaten;
            return 1f;
        }
        public TextureAtlasPosition this[string textureCode]
        {
            get
            {
                return this.fertilizerTexturePos;
            }
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            mesher.AddMeshData(this.fertilizerQuad, 1);
            currentRightMesh = GenRightMesh();
            if (this.currentRightMesh != null)
            {
                mesher.AddMeshData(this.currentRightMesh);
            }
            return false;
        }
        internal MeshData GenRightMesh()
        {
            MeshData fuelmesh = null;
            /*if (weedStage != WeedStage.NONE)
            {
                Shape shape = null;

                if (weedStage == WeedStage.LOW)
                {
                    shape = Api.Assets.TryGet("cancrops:shapes/weed-1.json").ToObject<Shape>();
                }
                else if (weedStage == WeedStage.MEDIUM)
                {
                    shape = Api.Assets.TryGet("cancrops:shapes/weed-2.json").ToObject<Shape>();
                }
                else if (weedStage == WeedStage.HIGH)
                {
                    shape = Api.Assets.TryGet("cancrops:shapes/weed-3.json").ToObject<Shape>();
                }

                if (shape != null)
                {
                    (Api as ICoreClientAPI).Tesselator.TesselateShape(this.Block, shape, out fuelmesh);
                }
            }*/
            if (cropSticksVariant > EnumCropSticksVariant.NONE)
            {
                Shape shape = null;

                MeshData tmp = null;
                if (cropSticksVariant == EnumCropSticksVariant.SINGLE)
                {
                    shape = Api.Assets.TryGet("cancrops:shapes/selection_sticks.json").ToObject<Shape>();
                }
                else if(cropSticksVariant == EnumCropSticksVariant.DOUBLE)
                {
                    shape = Api.Assets.TryGet("cancrops:shapes/selection_sticks_2.json").ToObject<Shape>();
                }
                
                if (shape != null)
                {
                    (Api as ICoreClientAPI).Tesselator.TesselateShape(this.Block, shape, out fuelmesh);
                }
                /*if (fuelmesh != null)
                {
                    fuelmesh.AddMeshData(tmp);
                }
                else
                {
                    fuelmesh = tmp;
                }*/
            }
            if (fuelmesh != null)
                return fuelmesh.Translate(new Vintagestory.API.MathTools.Vec3f(0, 0.9f, 0));
            return fuelmesh;
        }

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            if (this.Api.Side == EnumAppSide.Server)
            {
                this.Api.ModLoader.GetModSystem<POIRegistry>(true).RemovePOI(this);
            }
        }

        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            ICoreAPI api = this.Api;
            if (api != null && api.Side == EnumAppSide.Server)
            {
                this.Api.ModLoader.GetModSystem<POIRegistry>(true).RemovePOI(this);
            }
        }
        public void OnNeighbourBroken(BlockFacing facing)
        {
            neighbours.Remove(facing);
        }
        public void OnNeighbouropPlaced(BlockFacing facing, CANBlockEntityFarmland be)
        {
            neighbours[facing] = be;
        }
        public bool spawnGenome(Genome genome, AgriPlant agriPlant)
        {
            if (!this.hasPlant())
            {
                this.setGenomeImpl(genome, agriPlant);
                return true;
            }
            return false;
        }
        public bool hasPlant()
        {
            return this.agriPlant != null;
        }
        protected void setGenomeImpl(Genome genome, AgriPlant agriPlant)
        {
            
            Block block = this.Api.World.GetBlock(new AssetLocation(agriPlant.Domain + ":crop-" + agriPlant.Id + "-1"));

            if (block == null)
            {
                return;
            }
            this.Genome = genome;
            this.agriPlant = agriPlant;

            this.Api.World.BlockAccessor.SetBlock(block.BlockId, this.upPos);
            this.totalHoursForNextStage = this.Api.World.Calendar.TotalHours + this.GetHoursForNextStage();
            CropBehavior[] behaviors = block.CropProps.Behaviors;
            for (int i = 0; i < behaviors.Length; i++)
            {
                behaviors[i].OnPlanted(this.Api);
            }
            this.cropSticksVariant = EnumCropSticksVariant.NONE;
            this.MarkDirty(true);
        }

        //ATTRIBUTES, GETTERS

        MeshData currentRightMesh;

        public Genome Genome;

        public AgriPlant agriPlant;

        protected static Random rand = new Random();

        public static OrderedDictionary<string, float> Fertilities = new OrderedDictionary<string, float>
        {
            {
                "verylow",
                5f
            },
            {
                "low",
                25f
            },
            {
                "medium",
                50f
            },
            {
                "compost",
                65f
            },
            {
                "high",
                80f
            }
        };
        
        protected HashSet<string> PermaBoosts = new HashSet<string>();

        protected double totalHoursWaterRetention = 24.5;

        protected BlockPos upPos;

        protected double totalHoursForNextStage;

        protected double totalHoursLastUpdate;

        protected float[] nutrients = new float[3];

        protected float[] slowReleaseNutrients = new float[3];

        protected Dictionary<string, float> fertilizerOverlayStrength;

        protected float moistureLevel;

        protected double lastWaterSearchedTotalHours;

        protected TreeAttribute cropAttrs = new TreeAttribute();

        public int[] originalFertility = new int[3];

        protected bool unripeCropColdDamaged;

        protected bool unripeHeatDamaged;

        protected bool ripeCropColdDamaged;

        protected bool saltExposed;

        protected float[] damageAccum = new float[Enum.GetValues(typeof(EnumCropStressType)).Length];

        private CANBlockFarmland blockFarmland;

        protected Vec3d tmpPos = new Vec3d();

        protected float lastWaterDistance = 99f;

        protected double lastMoistureLevelUpdateTotalDays;

        public int roomness;

        protected bool allowundergroundfarming;

        protected bool allowcropDeath;

        protected float fertilityRecoverySpeed = 0.25f;

        protected float growthRateMul = 1f;

        protected MeshData fertilizerQuad;

        protected TextureAtlasPosition fertilizerTexturePos;

        private ICoreClientAPI capi;

        private bool farmlandIsAtChunkEdge;
        public Vec3d Position
        {
            get
            {
                return this.Pos.ToVec3d().Add(0.5, 1.0, 0.5);
            }
        }

        public string Type
        {
            get
            {
                return "food";
            }
        }

        BlockPos IFarmlandBlockEntity.Pos
        {
            get
            {
                return this.Pos;
            }
        }

        public new Size2i AtlasSize
        {
            get
            {
                return this.capi.BlockTextureAtlas.Size;
            }
        }

        protected enum EnumWaterSearchResult
        {
            Found,
            NotFound,

            Deferred
        }
        public double TotalHoursForNextStage
        {
            get
            {
                return this.totalHoursForNextStage;
            }
        }

        public double TotalHoursFertilityCheck
        {
            get
            {
                return this.totalHoursLastUpdate;
            }
        }

        public float[] Nutrients
        {
            get
            {
                return this.nutrients;
            }
        }

        public float MoistureLevel
        {
            get
            {
                return this.moistureLevel;
            }
        }

        public int[] OriginalFertility
        {
            get
            {
                return this.originalFertility;
            }
        }

        public BlockPos UpPos
        {
            get
            {
                return this.upPos;
            }
        }

        public ITreeAttribute CropAttributes
        {
            get
            {
                return this.cropAttrs;
            }
        }
        public bool IsVisiblyMoist
        {
            get
            {
                return (double)this.moistureLevel > 0.1;
            }
        }
    }

}
