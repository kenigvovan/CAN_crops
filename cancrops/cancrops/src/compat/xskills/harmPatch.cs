using cancrops.src.blockenities;
using cancrops.src.templates;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using XLib.XLeveling;
using XSkills;

namespace cancrops.src.compat.xskills
{
    [HarmonyPatch]
    public class harmPatch
    {
        public static void Postfix_DoHarmonyPatch(ICoreAPI api)
        {
            var or = typeof(BlockWateringCan).GetMethod("OnHeldInteractStep");
            //var patches = Harmony.GetPatchInfo(or);
            var harmonyInstance = new Harmony("tmp.cancrops");
            harmonyInstance.Unpatch(or, HarmonyPatchType.Postfix);
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
        public static bool Prefix_XSkillsItemPlantableSeed_OnHeldInteractStart(XSkillsItemPlantableSeed __instance, ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ICoreAPI ___api)
        {
            if (blockSel == null)
            {
                return false;
            }
            IPlayer player = (byEntity as EntityPlayer).Player;
            string str = itemslot.Itemstack.Collectible.LastCodePart(0);

            if (___api.Side == EnumAppSide.Client)
            {
                handHandling = EnumHandHandling.PreventDefault;
                return false;
            }
            
            AgriPlant agriPlant = cancrops.GetPlants().getPlant(itemslot.Itemstack.Collectible.Code.Domain + ":" + str);
            if(agriPlant == null)
            {
                return false;
            }
            // Block block = byEntity.World.GetBlock(base.CodeWithPath("crop-" + str + "-1"));
            Block block = byEntity.World.GetBlock((new AssetLocation(agriPlant.Domain + ":crop-" + str + "-1")));
            if (block == null)
            {
                return false;
            }
            XLeveling xleveling = XLeveling.Instance(___api);
            Farming farming = ((xleveling != null) ? xleveling.GetSkill("farming", false) : null) as Farming;
            if (farming == null)
            {
                Stub_ItemPlantableSeed_OnHeldInteractStart(__instance, itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
                return false;
            }
            PlayerSkillSet behavior = byEntity.GetBehavior<PlayerSkillSet>();
            PlayerSkill playerSkill = (behavior != null) ? behavior[farming.Id] : null;
            if (playerSkill == null)
            {
                Stub_ItemPlantableSeed_OnHeldInteractStart(__instance, itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
                return false;
            }
            PlayerAbility playerAbility = playerSkill[farming.ExtensiveFarmingId];
            int toolMode = __instance.GetToolMode(itemslot, player, blockSel);
            int num = 1;
            if (playerAbility != null && toolMode > 0)
            {
                num = playerAbility.Ability.Value(toolMode, 0);
            }
            int num2 = blockSel.Position.X;
            int y = blockSel.Position.Y;
            int num3 = blockSel.Position.Z;
            int num4 = 0;
            int num5 = 0;
            if (num % 2 == 0)
            {
                if ((double)num2 - byEntity.Pos.X >= 0.0)
                {
                    num4 = 1;
                }
                if ((double)num3 - byEntity.Pos.Z >= 0.0)
                {
                    num5 = 1;
                }
            }
            num2 = num2 - num / 2 + num4;
            num3 = num3 - num / 2 + num5;
            AssetLocation assetLocation = null;
            int num6 = num2;
            while (num6 < num2 + num && itemslot.StackSize > 0)
            {
                int num7 = num3;
                while (num7 < num3 + num && itemslot.StackSize > 0)
                {
                    BlockPos blockPos = new BlockPos(num6, y, num7);
                    CANBlockEntityFarmland blockEntityFarmland = byEntity.World.BlockAccessor.GetBlockEntity(blockPos) as CANBlockEntityFarmland;
                    if (blockEntityFarmland != null && blockEntityFarmland.TryPlant(block, itemslot.Itemstack, agriPlant))
                    {
                        handHandling = EnumHandHandling.PreventDefault;
                        if (assetLocation == null)
                        {
                            assetLocation = new AssetLocation("sounds/block/plant");
                        }
                        IClientPlayer clientPlayer = player as IClientPlayer;
                        if (clientPlayer != null)
                        {
                            clientPlayer.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
                        }
                        bool flag;
                        if (player == null)
                        {
                            flag = true;
                        }
                        else
                        {
                            IWorldPlayerData worldData = player.WorldData;
                            EnumGameMode? enumGameMode = (worldData != null) ? new EnumGameMode?(worldData.CurrentGameMode) : null;
                            EnumGameMode enumGameMode2 = EnumGameMode.Creative;
                            flag = !(enumGameMode.GetValueOrDefault() == enumGameMode2 & enumGameMode != null);
                        }
                        if (flag)
                        {
                            itemslot.TakeOut(1);
                            itemslot.MarkDirty();
                        }
                        playerAbility = playerSkill[farming.CultivatedSeedsId];
                        if (playerAbility != null && playerAbility.Tier > 0)
                        {
                            if (blockEntityFarmland.roomness > 0)
                            {
                                blockEntityFarmland.TryGrowCrop(___api.World.Calendar.TotalHours);
                            }
                            if (byEntity.World.Rand.NextDouble() < (double)playerAbility.SkillDependentFValue(0))
                            {
                                blockEntityFarmland.TryGrowCrop(___api.World.Calendar.TotalHours);
                            }
                        }
                    }
                    num7++;
                }
                num6++;
            }
            if (assetLocation != null)
            {
                byEntity.World.PlaySoundAt(assetLocation, (double)blockSel.Position.X, (double)blockSel.Position.Y, (double)blockSel.Position.Z, null, true, 32f, 1f);
            }
            return false;
        }
        public static void Stub_ItemPlantableSeed_OnHeldInteractStart(object instance, ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
        {
            // its a stub so it has no initial content
            throw new NotImplementedException("It's a stub");
            // (instance as Block).OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        }
        public static bool Prefix_XSkillsItemHoe_DoTill(XSkillsItemHoe __instance, float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ICoreAPI ___api)
        {
            if (blockSel == null)
            {
                return false;
            }
            IPlayer player = (byEntity as EntityPlayer).Player;
            XLeveling xleveling = XLeveling.Instance(___api);
            Farming farming = ((xleveling != null) ? xleveling.GetSkill("farming", false) : null) as Farming;
            if (farming == null)
            {
                Stub_XSkillsItemHoe_DoTill(__instance, secondsUsed, slot, byEntity, blockSel, entitySel);
                return false;
            }
            EntityPlayer entity = player.Entity;
            PlayerAbility playerAbility;
            if (entity == null)
            {
                playerAbility = null;
            }
            else
            {
                PlayerSkill playerSkill = entity.GetBehavior<PlayerSkillSet>()[farming.Id];
                playerAbility = ((playerSkill != null) ? playerSkill[farming.ExtensiveFarmingId] : null);
            }
            PlayerAbility playerAbility2 = playerAbility;
            if (playerAbility2 == null || playerAbility2.Tier <= 0)
            {
                LocalDoTill(secondsUsed, slot, byEntity, blockSel, entitySel, ___api);
                return false;
            }
            int toolMode = __instance.GetToolMode(slot, player, blockSel);
            if (toolMode <= 0)
            {
                LocalDoTill(secondsUsed, slot, byEntity, blockSel, entitySel, ___api);
                return false;
            }
            int num = playerAbility2.Ability.Value(toolMode, 0);
            int num2 = 0;
            int num3 = blockSel.Position.X;
            int y = blockSel.Position.Y;
            int num4 = blockSel.Position.Z;
            int num5 = 0;
            int num6 = 0;
            if (num % 2 == 0)
            {
                if ((double)num3 - byEntity.Pos.X >= 0.0)
                {
                    num5 = 1;
                }
                if ((double)num4 - byEntity.Pos.Z >= 0.0)
                {
                    num6 = 1;
                }
            }
            num3 = num3 - num / 2 + num5;
            num4 = num4 - num / 2 + num6;
            AssetLocation assetLocation = null;
            for (int i = num3; i < num3 + num; i++)
            {
                for (int j = num4; j < num4 + num; j++)
                {
                    Block block = ___api.World.BlockAccessor.GetBlock(i, y + 1, j);
                    if (block != null && block.Id == 0)
                    {
                        block = ___api.World.BlockAccessor.GetBlock(i, y, j);
                        if (block.Code.Path.StartsWith("soil"))
                        {
                            string str = block.LastCodePart(1);
                            Block block2 = byEntity.World.GetBlock(new AssetLocation("cancrops:canfarmland-dry-" + str));
                            if (block2 != null)
                            {
                                BlockPos blockPos = new BlockPos(i, y, j);
                                ___api.World.BlockAccessor.SetBlock(block2.BlockId, blockPos);
                                num2++;
                                BlockEntity blockEntity = byEntity.World.BlockAccessor.GetBlockEntity(blockPos);
                                if (blockEntity is CANBlockEntityFarmland)
                                {
                                    ((CANBlockEntityFarmland)blockEntity).OnCreatedFromSoil(block);
                                }
                                ___api.World.BlockAccessor.MarkBlockDirty(blockPos);
                                if (player != null && block.Sounds != null && assetLocation == null)
                                {
                                    assetLocation = block.Sounds.Place;
                                }
                            }
                        }
                    }
                }
            }
            num2 = (int)((float)num2 * 0.5f + 0.6f);
            if (num2 > 0)
            {
                slot.Itemstack.Collectible.DamageItem(byEntity.World, byEntity, player.InventoryManager.ActiveHotbarSlot, num2);
            }
            if (slot.Empty)
            {
                byEntity.World.PlaySoundAt(new AssetLocation("sounds/effect/toolbreak"), byEntity.Pos.X, byEntity.Pos.Y, byEntity.Pos.Z, null, true, 32f, 1f);
            }
            if (assetLocation != null)
            {
                byEntity.World.PlaySoundAt(assetLocation, (double)blockSel.Position.X, (double)blockSel.Position.Y, (double)blockSel.Position.Z, null, true, 32f, 1f);
            }
            return false;
        }
        public static void Stub_XSkillsItemHoe_DoTill(object __instance, float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            // its a stub so it has no initial content
            throw new NotImplementedException("It's a stub");
            // (instance as Block).OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        }
        public static void LocalDoTill(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ICoreAPI ___api)
        {
            BlockPos pos = blockSel.Position;
            Block block = byEntity.World.BlockAccessor.GetBlock(pos);
            if (!block.Code.Path.StartsWith("soil"))
            {
                return;
            }
            string fertility = block.LastCodePart(1);
            Block farmland = byEntity.World.GetBlock(new AssetLocation("cancrops:canfarmland-dry-" + fertility));
            IPlayer byPlayer = (byEntity as EntityPlayer).Player;
            if (farmland == null || byPlayer == null)
            {
                return;
            }
            if (block.Sounds != null)
            {
                byEntity.World.PlaySoundAt(block.Sounds.Place, (double)pos.X, (double)pos.Y, (double)pos.Z, null, true, 32f, 1f);
            }
            byEntity.World.BlockAccessor.SetBlock(farmland.BlockId, pos);
            slot.Itemstack.Collectible.DamageItem(byEntity.World, byEntity, byPlayer.InventoryManager.ActiveHotbarSlot, 1);
            if (slot.Empty)
            {
                byEntity.World.PlaySoundAt(new AssetLocation("sounds/effect/toolbreak"), byEntity.Pos.X, byEntity.Pos.Y, byEntity.Pos.Z, null, true, 32f, 1f);
            }
            BlockEntity be = byEntity.World.BlockAccessor.GetBlockEntity(pos);
            if (be is CANBlockEntityFarmland)
            {
                ((CANBlockEntityFarmland)be).OnCreatedFromSoil(block);
            }
            byEntity.World.BlockAccessor.MarkBlockDirty(pos);
            return;
        }
    }
}
