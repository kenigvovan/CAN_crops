using cancrops.src.blockenities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace cancrops.src.blocks
{
    public class CANBlockFarmland : BlockFarmland
    {
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            if (this.Attributes != null)
            {
                this.DelayGrowthBelowSunLight = this.Attributes["delayGrowthBelowSunLight"].AsInt(19);
                this.LossPerLevel = this.Attributes["lossPerLevel"].AsFloat(0.1f);
                if (this.WeedNames == null)
                {
                    this.WeedNames = this.Attributes["weedBlockCodes"].AsObject<CodeAndChance[]>(null);
                    int i = 0;
                    while (this.WeedNames != null && i < this.WeedNames.Length)
                    {
                        this.TotalWeedChance += this.WeedNames[i].Chance;
                        i++;
                    }
                }
            }
            this.wsys = api.ModLoader.GetModSystem<WeatherSystemBase>(true);
            this.roomreg = api.ModLoader.GetModSystem<RoomRegistry>(true);
        }
        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(world, blockPos, byItemStack);
            if (byItemStack != null)
            {
                CANBlockEntityFarmland blockEntityFarmland = world.BlockAccessor.GetBlockEntity(blockPos) as CANBlockEntityFarmland;
                if (blockEntityFarmland == null)
                {
                    return;
                }
                blockEntityFarmland.OnCreatedFromSoil(byItemStack.Block);


            }
            //when be farmland is broken we remove neighbor from other neighbours dicts
            foreach (var facing in BlockFacing.HORIZONTALS)
            {
                CANBlockEntityFarmland neighborBE = world.BlockAccessor.GetBlockEntity(blockPos.Copy().Offset(facing)) as CANBlockEntityFarmland;
                if (neighborBE == null)
                {
                    continue;
                }
                neighborBE.OnNeighbouropPlaced(facing.Opposite, neighborBE);
            }
        }
        public override bool CanAttachBlockAt(IBlockAccessor world, Block block, BlockPos pos, BlockFacing blockFace, Cuboidi attachmentArea = null)
        {
            return ((block is BlockCrop || block is BlockDeadCrop) && blockFace == BlockFacing.UP) || (!blockFace.IsHorizontal && base.CanAttachBlockAt(world, block, pos, blockFace, attachmentArea));
        }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            CANBlockEntityFarmland befarmland = world.BlockAccessor.GetBlockEntity(blockSel.Position) as CANBlockEntityFarmland;
            if(befarmland != null)
            {
                if(byPlayer.InventoryManager.ActiveHotbarSlot?.Itemstack?.Item?.Tool == EnumTool.Hoe)
                {
                    if(befarmland.GetCropSticksVariant() != utility.EnumCropSticksVariant.NONE)
                    {
                        befarmland.OnHoeUsed();
                        return true;
                    }
                }
            }
            return (befarmland != null && befarmland.OnBlockInteract(byPlayer)) || base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
        public override string GetPlacedBlockName(IWorldAccessor world, BlockPos pos)
        {
            CANBlockEntityFarmland befarmland = world.BlockAccessor.GetBlockEntity(pos) as CANBlockEntityFarmland;
            if (befarmland != null)
            {
                return new ItemStack(this.api.World.GetBlock(base.CodeWithVariant("state", befarmland.IsVisiblyMoist ? "moist" : "dry")), 1).GetName();
            }
            return base.GetPlacedBlockName(world, pos);
        }
        public override int GetRetention(BlockPos pos, BlockFacing facing, EnumRetentionType type)
        {
            return 3;
        }
        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
            //when be farmland is broken we remove neighbor from other neighbours dicts
            foreach (var facing in BlockFacing.HORIZONTALS)
            {
                CANBlockEntityFarmland blockEntityFarmland = world.BlockAccessor.GetBlockEntity(pos.Copy().Offset(facing)) as CANBlockEntityFarmland;
                if (blockEntityFarmland == null)
                {
                    continue;
                }
                blockEntityFarmland.OnNeighbourBroken(facing.Opposite);
            }
        }
        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            base.OnNeighbourBlockChange(world, pos, neibpos);
        }
    }
}
