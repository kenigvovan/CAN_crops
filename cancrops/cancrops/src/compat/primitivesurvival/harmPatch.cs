using cancrops.src.blockenities;
using HarmonyLib;
using PrimitiveSurvival.ModSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace cancrops.src.compat.primitivesurvival
{
    [HarmonyPatch]
    public class harmPatch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(EntityAgent), "OnGameTick")]
        public static void Stub_OnGameTick(object instance, float dt)
        {
            // its a stub so it has no initial content
            throw new NotImplementedException("It's a stub");
            // (instance as Block).OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        }
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(CollectibleObject), "OnHeldInteractStart")]
        public static void Stub_Item_OnHeldInteractStart(object instance, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
        {
            // its a stub so it has no initial content
            throw new NotImplementedException("It's a stub");
            // (instance as Block).OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        }

        public static bool Prefix_ItemEarthworm_OnHeldInteractStart(ItemEarthworm __instance, ItemSlot slot,
            EntityAgent byEntity, BlockSelection blockSel,
            EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling,
            ICoreAPI ___api)
        {
            if (byEntity.Controls.Sneak && blockSel != null)
            {
                IPlayer player = byEntity.World.PlayerByUid((byEntity as EntityPlayer).PlayerUID);
                if (!byEntity.World.Claims.TryAccess(player, blockSel.Position, EnumBlockAccessFlags.BuildOrBreak))
                {
                    return false;
                }
                if (!(byEntity is EntityPlayer) || player.WorldData.CurrentGameMode != EnumGameMode.Creative)
                {
                    slot.TakeOut(1);
                    slot.MarkDirty();
                }
                AssetLocation assetLocation = new AssetLocation(__instance.Code.Domain, (__instance as Item).CodeEndWithoutParts(1));
                EntityProperties entityType = byEntity.World.GetEntityType(assetLocation);
                if (entityType == null)
                {
                    byEntity.World.Logger.Error("ItemCreature: No such entity - {0}", new object[]
                    {
                        assetLocation
                    });

                    if (___api.World.Side == EnumAppSide.Client)
                    {
                        (___api as ICoreClientAPI).TriggerIngameError(__instance, "nosuchentity", "No such entity '{0}' loaded.");
                    }
                    return false;
                }
                Entity entity = byEntity.World.ClassRegistry.CreateEntity(entityType);
                if (entity != null)
                {
                    entity.ServerPos.X = (double)((float)(blockSel.Position.X + (blockSel.DidOffset ? 0 : blockSel.Face.Normali.X)) + 0.5f);
                    entity.ServerPos.Y = (double)(blockSel.Position.Y + (blockSel.DidOffset ? 0 : blockSel.Face.Normali.Y));
                    entity.ServerPos.Z = (double)((float)(blockSel.Position.Z + (blockSel.DidOffset ? 0 : blockSel.Face.Normali.Z)) + 0.5f);
                    entity.ServerPos.Yaw = (float)byEntity.World.Rand.NextDouble() * 2f * 3.1415927f;
                    entity.Pos.SetFrom(entity.ServerPos);
                    entity.PositionBeforeFalling.Set(entity.ServerPos.X, entity.ServerPos.Y, entity.ServerPos.Z);
                    entity.Attributes.SetString("origin", "playerplaced");
                    byEntity.World.SpawnEntity(entity);
                    handHandling = EnumHandHandling.PreventDefaultAction;
                    return false;
                }
            }
            else
            {
                Stub_Item_OnHeldInteractStart(__instance, slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
            }
            return false;
        }
        public static bool Prefix_OnGameTick(EntityEarthworm __instance, float dt, ref int ___cnt, ref Random ___Rnd)
        {
            Stub_OnGameTick(__instance, dt);
            ___cnt++;
            bool flag = ___cnt > 200;
            if (flag)
            {
                ___cnt = 0;
                BlockPos asBlockPos = __instance.Pos.XYZ.AsBlockPos;
                Block block = __instance.World.BlockAccessor.GetBlock(asBlockPos, 0);
                ClimateCondition climateAt = __instance.World.BlockAccessor.GetClimateAt(asBlockPos, EnumGetClimateMode.NowValues, 0.0);
                int num2 = ___Rnd.Next(200);
                bool flag2 = climateAt.Temperature <= 0f || climateAt.Temperature >= 35f || num2 < 1;
                if (flag2)
                {
                    __instance.Die(0, null);
                }
                else
                {
                    var c = block.FirstCodePart(0);
                    bool flag3 = block.FirstCodePart(0) == "canfarmland";
                    if (flag3)
                    {
                        CANBlockEntityFarmland blockEntityFarmland = __instance.World.BlockAccessor.GetBlockEntity(asBlockPos) as CANBlockEntityFarmland;
                        bool flag4 = blockEntityFarmland != null;
                        if (flag4)
                        {
                            bool flag5 = EnumAppSideExtensions.IsServer(__instance.Api.Side);
                            if (flag5)
                            {
                                if (blockEntityFarmland != null)
                                {
                                    blockEntityFarmland.WaterFarmland(0.3f, true);
                                }
                                TreeAttribute treeAttribute = new TreeAttribute();
                                if (blockEntityFarmland != null)
                                {
                                    blockEntityFarmland.ToTreeAttributes(treeAttribute);
                                }
                                bool flag6 = treeAttribute != null;
                                if (flag6)
                                {
                                    float num3 = treeAttribute.GetFloat("slowN", 0f);
                                    float num4 = treeAttribute.GetFloat("slowK", 0f);
                                    float num5 = treeAttribute.GetFloat("slowP", 0f);
                                    bool flag7 = num3 <= 150f;
                                    if (flag7)
                                    {
                                        num3 += 1f;
                                    }
                                    bool flag8 = num4 <= 150f;
                                    if (flag8)
                                    {
                                        num4 += 1f;
                                    }
                                    bool flag9 = num5 <= 150f;
                                    if (flag9)
                                    {
                                        num5 += 1f;
                                    }
                                    bool flag10 = num3 < 150f && num4 < 150f && num5 < 150f;
                                    if (flag10)
                                    {
                                        treeAttribute.SetFloat("slowN", num3);
                                        treeAttribute.SetFloat("slowK", num4);
                                        treeAttribute.SetFloat("slowP", num5);
                                        if (blockEntityFarmland != null)
                                        {
                                            blockEntityFarmland.FromTreeAttributes(treeAttribute, __instance.World);
                                        }
                                        if (blockEntityFarmland != null)
                                        {
                                            blockEntityFarmland.MarkDirty(false, null);
                                        }
                                        __instance.World.BlockAccessor.MarkBlockEntityDirty(asBlockPos);
                                    }
                                    else
                                    {
                                        __instance.World.BlockAccessor.BreakBlock(asBlockPos, null, 1f);
                                        Block block2 = __instance.World.BlockAccessor.GetBlock(new AssetLocation("primitivesurvival:earthwormcastings"));
                                        __instance.World.BlockAccessor.SetBlock(block2.BlockId, asBlockPos);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
        public static bool Prefix_TryPlaceBlock(BlockSupport __instance, IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode, ICoreAPI ___api)
        {
            BlockPos blockPos = blockSel.Position.Copy();
            blockPos.Y--;
            Block block = world.BlockAccessor.GetBlock(blockPos, 0);
            bool flag = !block.Code.Path.Contains("crop") && block.Fertility <= 0 && !block.Code.Path.Contains("canfarmland");
            bool result;
            if (flag)
            {
                failureCode = Lang.Get("you need more suitable ground to place this support", Array.Empty<object>());
                result = false;
            }
            else
            {
                string str = itemstack.Collectible.FirstCodePart(1);
                string str2 = "";
                BlockPos position = blockSel.Position;
                bool flag2 = block.Code.Path.Contains("crop");
                if (flag2)
                {
                    position.Y--;
                }
                BlockPos[] array = null;
                string a = Block.SuggestedHVOrientation(byPlayer, blockSel)[0].ToString();
                bool flag3 = a == "north";
                if (flag3)
                {
                    block = world.BlockAccessor.GetBlock(position.NorthCopy(1).DownCopy(1), 0);
                    array = new BlockPos[]
                    {
                        position.UpCopy(3),
                        position.UpCopy(3).NorthCopy(1)
                    };
                    str2 = "ns";
                }
                else
                {
                    bool flag4 = a == "east";
                    if (flag4)
                    {
                        block = world.BlockAccessor.GetBlock(position.EastCopy(1).DownCopy(1), 0);
                        array = new BlockPos[]
                        {
                            position.UpCopy(2),
                            position.UpCopy(2).EastCopy(1)
                        };
                        str2 = "ew";
                    }
                    else
                    {
                        bool flag5 = a == "south";
                        if (flag5)
                        {
                            block = world.BlockAccessor.GetBlock(position.SouthCopy(1).DownCopy(1), 0);
                            array = new BlockPos[]
                            {
                                position.UpCopy(3).SouthCopy(1),
                                position.UpCopy(3)
                            };
                            str2 = "ns";
                        }
                        else
                        {
                            bool flag6 = a == "west";
                            if (flag6)
                            {
                                block = world.BlockAccessor.GetBlock(position.WestCopy(1).DownCopy(1), 0);
                                array = new BlockPos[]
                                {
                                    position.UpCopy(2).WestCopy(1),
                                    position.UpCopy(2)
                                };
                                str2 = "ew";
                            }
                        }
                    }
                }
                bool flag7 = !block.Code.Path.Contains("crop") && block.Fertility <= 0 && !block.Code.Path.Contains("canfarmland");
                if (flag7)
                {
                    failureCode = Lang.Get("you need more suitable ground to place this support", Array.Empty<object>());
                    result = false;
                }
                else
                {
                    foreach (BlockPos blockPos2 in array)
                    {
                        Block block2 = ___api.World.BlockAccessor.GetBlock(blockPos2, 0);
                        bool flag8 = block2.BlockId != 0;
                        if (flag8)
                        {
                            return false;
                        }
                    }
                    int num = 0;
                    foreach (BlockPos blockPos3 in array)
                    {
                        Block block3 = ___api.World.BlockAccessor.GetBlock(blockPos3, 0);
                        string text = "primitivesurvival:support-" + str + "-";
                        bool flag9 = num == 0;
                        if (flag9)
                        {
                            text += "main";
                        }
                        else
                        {
                            text += "empty";
                        }
                        text = text + "-" + str2;
                        Block block4 = ___api.World.GetBlock(new AssetLocation(text));
                        bool flag10 = block4 != null;
                        if (flag10)
                        {
                            ___api.World.BlockAccessor.SetBlock(block4.BlockId, blockPos3);
                        }
                        num++;
                    }
                    result = true;
                }
            }
            return result;
        }
        public static bool Prefix_TryWaterFarmland(BESupport __instance, BlockPos pos, string dir)
        {
            bool flag = dir == "ns";
            BlockPos blockPos;
            int num;
            if (flag)
            {
                blockPos = pos.NorthCopy(1);
                num = 6;
            }
            else
            {
                blockPos = pos.EastCopy(1);
                num = 5;
            }
            BlockPos[] array = new BlockPos[]
            {
                pos,
                blockPos
            };
            foreach (BlockPos blockPos2 in array)
            {
                bool flag2 = false;
                int num2 = 0;
                BlockPos blockPos3 = blockPos2.DownCopy(1);
                do
                {
                    Block block = __instance.Api.World.BlockAccessor.GetBlock(blockPos3, 0);
                    bool flag3 = block.Id != 0 && block.FirstCodePart(0) != "support";
                    if (flag3)
                    {
                        flag2 = true;
                        bool flag4 = block.Class == "BlockCrop" || block.Class == "BlockTallGrass" || block.Class == "BlockPlant";
                        if (flag4)
                        {
                            blockPos3 = blockPos3.DownCopy(1);
                            block = __instance.Api.World.BlockAccessor.GetBlock(blockPos3, 0);
                        }
                        CANBlockEntityFarmland blockEntityFarmland = __instance.Api.World.BlockAccessor.GetBlockEntity(blockPos3) as CANBlockEntityFarmland;
                        bool flag5 = blockEntityFarmland != null;
                        if (flag5)
                        {
                            bool flag6 = (double)blockEntityFarmland.MoistureLevel <= 0.99;
                            if (flag6)
                            {
                                blockEntityFarmland.WaterFarmland(0.1f, false);
                            }
                        }
                    }
                    num2++;
                    blockPos3 = blockPos3.DownCopy(1);
                }
                while (!flag2 && num2 < num);
            }
            return false;
        }
        public static bool Prefix_IrrigationVesselUpdate(BEIrrigationVessel __instance, float par, InventoryGeneric ___inventory)
        {
            bool flag = ___inventory[0].Empty && !__instance.Buried;
            if (flag)
            {
                __instance.Api.World.BlockAccessor.SetBlock(0, __instance.Pos, 2);
            }
            else
            {
                bool flag2 = ___inventory[0].Empty || !__instance.Buried;
                if (!flag2)
                {
                    bool flag3 = !___inventory[0].Itemstack.Collectible.Code.Path.Contains("water");
                    if (!flag3)
                    {
                        bool flag4 = ___inventory[0].Itemstack.Collectible.Code.Path.Contains("saltwater");
                        if (!flag4)
                        {
                            string text = "game:water-still-6";
                            Block block = __instance.Api.World.GetBlock(new AssetLocation(text));
                            __instance.Api.World.BlockAccessor.SetBlock(block.BlockId, __instance.Pos, 2);
                            BlockPos[] array = new BlockPos[]
                            {
                                __instance.Pos.EastCopy(1),
                                __instance.Pos.EastCopy(1).NorthCopy(1),
                                __instance.Pos.NorthCopy(1),
                                __instance.Pos.NorthCopy(1).WestCopy(1),
                                __instance.Pos.SouthCopy(1),
                                __instance.Pos.SouthCopy(1).EastCopy(1),
                                __instance.Pos.WestCopy(1),
                                __instance.Pos.WestCopy(1).SouthCopy(1)
                            };
                            double num = 0.0;
                            foreach (BlockPos blockPos in array)
                            {
                                CANBlockEntityFarmland blockEntityFarmland = __instance.Api.World.BlockAccessor.GetBlockEntity(blockPos) as CANBlockEntityFarmland;
                                bool flag5 = blockEntityFarmland != null;
                                if (flag5)
                                {
                                    float num2 = 1f - blockEntityFarmland.MoistureLevel + 0.2f;
                                    blockEntityFarmland.WaterFarmland(num2, true);
                                    num += (double)(num2 * 2.5f);
                                    TreeAttribute treeAttribute = new TreeAttribute();
                                    blockEntityFarmland.ToTreeAttributes(treeAttribute);
                                }
                            }
                            ___inventory[0].TakeOut((int)num);
                            __instance.MarkDirty(true);
                        }
                    }
                }
            }
            return false;
        }
        public static bool Prefix_OnPipeTick(BEFurrowedLand __instance, float dt, double ___FurrowedLandMinMoistureFar, double ___FurrowedLandMinMoistureClose, Random ___Rnd, double ___FurrowedLandBlockageChancePercent, string[] ___blockageTypes)
        {
            Block block = __instance.Api.World.BlockAccessor.GetBlock(__instance.Pos, 2);
            bool flag = block.Code.Path.Contains("water") && __instance.OtherSlot.Empty && !block.Code.Path.Contains("saltwater");
            if (flag)
            {
                bool flag2 = ___Rnd.NextDouble() < ___FurrowedLandBlockageChancePercent / 100.0;
                if (flag2)
                {
                    __instance.OtherStack = new ItemStack(__instance.Api.World.GetItem(new AssetLocation("game:" + ___blockageTypes[___Rnd.Next(___blockageTypes.Count<string>())])), ___Rnd.Next(3) + 1);
                    __instance.Api.World.BlockAccessor.SetBlock(0, __instance.Pos, 2);
                    __instance.MarkDirty(true, null);
                }
                else
                {
                    BlockPos[] array = new BlockPos[]
                    {
                        __instance.Pos.EastCopy(1),
                        __instance.Pos.NorthCopy(1),
                        __instance.Pos.SouthCopy(1),
                        __instance.Pos.WestCopy(1)
                    };
                    foreach (BlockPos blockPos in array)
                    {
                        CANBlockEntityFarmland blockEntityFarmland = __instance.Api.World.BlockAccessor.GetBlockEntity(blockPos) as CANBlockEntityFarmland;
                        bool flag3 = blockEntityFarmland != null;
                        if (flag3)
                        {
                            bool flag4 = (double)blockEntityFarmland.MoistureLevel < ___FurrowedLandMinMoistureClose;
                            if (flag4)
                            {
                                blockEntityFarmland.WaterFarmland(0.1f, false);
                            }
                            TreeAttribute treeAttribute = new TreeAttribute();
                            blockEntityFarmland.ToTreeAttributes(treeAttribute);
                        }
                    }
                    array = new BlockPos[]
                    {
                        __instance.Pos.EastCopy(1).EastCopy(1),
                        __instance.Pos.NorthCopy(1).NorthCopy(1),
                        __instance.Pos.SouthCopy(1).SouthCopy(1),
                        __instance.Pos.WestCopy(1).WestCopy(1)
                    };
                    foreach (BlockPos blockPos2 in array)
                    {
                        CANBlockEntityFarmland blockEntityFarmland2 = __instance.Api.World.BlockAccessor.GetBlockEntity(blockPos2) as CANBlockEntityFarmland;
                        bool flag5 = blockEntityFarmland2 != null;
                        if (flag5)
                        {
                            bool flag6 = (double)blockEntityFarmland2.MoistureLevel < ___FurrowedLandMinMoistureFar;
                            if (flag6)
                            {
                                blockEntityFarmland2.WaterFarmland(0.05f, false);
                            }
                            TreeAttribute treeAttribute2 = new TreeAttribute();
                            blockEntityFarmland2.ToTreeAttributes(treeAttribute2);
                        }
                    }
                }
            }
            return false;
        }
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ItemHoe), "OnHeldInteractStart")]
        public static void Stub_OnHeldInteractStart(object instance, ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
        {
            // its a stub so it has no initial content
            throw new NotImplementedException("It's a stub");
            // (instance as Block).OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        }
        public static bool Prefix_ItemHoeExtended_OnHeldInteractStart(ItemHoeExtended __instance, ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
        {
            bool flag = blockSel == null;
            if (!flag)
            {
                bool flag2 = byEntity.Controls.ShiftKey && byEntity.Controls.CtrlKey;
                if (flag2)
                {
                    Stub_OnHeldInteractStart(__instance, itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
                }
                else
                {
                    BlockPos position = blockSel.Position;
                    Block block = byEntity.World.BlockAccessor.GetBlock(position);
                    byEntity.Attributes.SetInt("didtill", 0);
                    bool flag3 = block.Code.Path.StartsWith("soil");
                    if (flag3)
                    {
                        handHandling = EnumHandHandling.PreventDefault;
                    }
                    bool flag4 = block.Code.Path.StartsWith("canfarmland");
                    if (flag4)
                    {
                        bool shiftKey = byEntity.Controls.ShiftKey;
                        if (shiftKey)
                        {
                            handHandling = EnumHandHandling.PreventDefault;
                        }
                    }
                }
            }
            return false;
        }
        public static bool Prefix_ItemHoeExtended_DoTill(ItemHoeExtended __instance, float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            bool flag = blockSel == null;
            if (!flag)
            {
                BlockPos position = blockSel.Position;
                Block block = byEntity.World.BlockAccessor.GetBlock(position);
                bool flag2 = block.Code.Path.StartsWith("farmland") || block.Code.Path.StartsWith("canfarmland");
                if (flag2)
                {
                    string text = "primitivesurvival:furrowedland-" + block.LastCodePart(0) + "-free";
                    Block block2 = byEntity.World.GetBlock(new AssetLocation(text));
                    byEntity.World.BlockAccessor.SetBlock(block2.BlockId, blockSel.Position, 0);
                    byEntity.World.BlockAccessor.MarkBlockDirty(position);
                    bool flag3 = block.Sounds != null;
                    if (flag3)
                    {
                        byEntity.World.PlaySoundAt(block.Sounds.Place, (double)position.X, (double)position.Y, (double)position.Z, null, true, 32f, 1f);
                    }
                    IPlayer player = (byEntity as EntityPlayer).Player;
                    slot.Itemstack.Collectible.DamageItem(byEntity.World, byEntity, player.InventoryManager.ActiveHotbarSlot, 1);
                    bool empty = slot.Empty;
                    if (empty)
                    {
                        byEntity.World.PlaySoundAt(new AssetLocation("game:sounds/effect/toolbreak"), byEntity.Pos.X, byEntity.Pos.Y, byEntity.Pos.Z, null, true, 32f, 1f);
                    }
                }
                else
                {
                    bool flag4 = !block.Code.Path.StartsWith("soil");
                    if (!flag4)
                    {
                        string str = block.LastCodePart(1);
                        Block block3 = byEntity.World.GetBlock(new AssetLocation("cancrops:canfarmland-dry-" + str));
                        IPlayer player2 = (byEntity as EntityPlayer).Player;
                        bool flag5 = block3 == null || player2 == null;
                        if (!flag5)
                        {
                            bool flag6 = block.Sounds != null;
                            if (flag6)
                            {
                                byEntity.World.PlaySoundAt(block.Sounds.Place, (double)position.X, (double)position.Y, (double)position.Z, null, true, 32f, 1f);
                            }
                            byEntity.World.BlockAccessor.SetBlock(block3.BlockId, position);
                            slot.Itemstack.Collectible.DamageItem(byEntity.World, byEntity, player2.InventoryManager.ActiveHotbarSlot, 1);
                            bool empty2 = slot.Empty;
                            if (empty2)
                            {
                                byEntity.World.PlaySoundAt(new AssetLocation("game:sounds/effect/toolbreak"), byEntity.Pos.X, byEntity.Pos.Y, byEntity.Pos.Z, null, true, 32f, 1f);
                            }
                            BlockEntity blockEntity = byEntity.World.BlockAccessor.GetBlockEntity(position);
                            CANBlockEntityFarmland blockEntityFarmland = blockEntity as CANBlockEntityFarmland;
                            bool flag7 = blockEntityFarmland != null;
                            if (flag7)
                            {
                                blockEntityFarmland.OnCreatedFromSoil(block);
                            }
                            byEntity.World.BlockAccessor.MarkBlockDirty(position);
                        }
                    }
                }
            }
            return false;
        }

    }
}
