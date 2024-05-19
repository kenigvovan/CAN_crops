using cancrops.src.blockenities;
using cancrops.src.blocks;
using cancrops.src.genetics;
using cancrops.src.templates;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.Common;
using Vintagestory.GameContent;
using weightmod.src;

namespace cancrops.src
{
    [HarmonyPatch]
    public class harmPatch
    {
        public static bool Prefix_BlockWateringCan_OnHeldInteractStep(BlockWateringCan __instance, float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ICoreAPI ___api, ref bool __result)
        {
            if (blockSel == null)
            {
                __result = false;
                return false;
            }
            if (slot.Itemstack.TempAttributes.GetInt("refilled", 0) > 0)
            {
                __result = false;
                return false;
            }
            float prevsecondsused = slot.Itemstack.TempAttributes.GetFloat("secondsUsed", 0f);
            slot.Itemstack.TempAttributes.SetFloat("secondsUsed", secondsUsed);
            float remainingwater = __instance.GetRemainingWateringSeconds(slot.Itemstack);
            __instance.SetRemainingWateringSeconds(slot.Itemstack, remainingwater -= secondsUsed - prevsecondsused);
            if (remainingwater <= 0f)
            {
                __result = false;
                return false;
            }
            IWorldAccessor world = byEntity.World;
            BlockPos targetPos = blockSel.Position;
            if (___api.World.Side == EnumAppSide.Server)
            {
                BlockEntity blockEntity = world.BlockAccessor.GetBlockEntity(blockSel.Position.AddCopy(blockSel.Face));
                BEBehaviorBurning beburningBh = (blockEntity != null) ? blockEntity.GetBehavior<BEBehaviorBurning>() : null;
                if (beburningBh != null)
                {
                    beburningBh.KillFire(false);
                }
                BlockEntity blockEntity2 = world.BlockAccessor.GetBlockEntity(blockSel.Position);
                beburningBh = ((blockEntity2 != null) ? blockEntity2.GetBehavior<BEBehaviorBurning>() : null);
                if (beburningBh != null)
                {
                    beburningBh.KillFire(false);
                }
                Vec3i voxelPos = new Vec3i();
                for (int dx = -2; dx < 2; dx++)
                {
                    for (int dy = -2; dy < 2; dy++)
                    {
                        for (int dz = -2; dz < 2; dz++)
                        {
                            int x = (int)(blockSel.HitPosition.X * 16.0);
                            int y = (int)(blockSel.HitPosition.Y * 16.0);
                            int z = (int)(blockSel.HitPosition.Z * 16.0);
                            if (x + dx >= 0 && x + dx <= 15 && y + dy >= 0 && y + dy <= 15 && z + dz >= 0 && z + dz <= 15)
                            {
                                voxelPos.Set(x + dx, y + dy, z + dz);
                                int faceAndSubPosition = CollectibleBehaviorArtPigment.BlockSelectionToSubPosition(blockSel.Face, voxelPos);
                                Block decor = world.BlockAccessor.GetDecor(blockSel.Position, faceAndSubPosition);
                                if (((decor != null) ? decor.FirstCodePart(0) : null) == "caveart")
                                {
                                    world.BlockAccessor.BreakDecor(blockSel.Position, blockSel.Face, new int?(faceAndSubPosition));
                                }
                            }
                        }
                    }
                }
            }
            Block block = world.BlockAccessor.GetBlock(blockSel.Position);
            bool notOnSolidblock = false;
            if (block.CollisionBoxes == null || block.CollisionBoxes.Length == 0)
            {
                block = world.BlockAccessor.GetBlock(blockSel.Position, 2);
                if ((block.CollisionBoxes == null || block.CollisionBoxes.Length == 0) && !block.IsLiquid())
                {
                    notOnSolidblock = true;
                    targetPos = targetPos.DownCopy(1);
                }
            }
            BlockEntity be =  world.BlockAccessor.GetBlockEntity(targetPos);
            if (be != null)
            {
                if (be is CANBlockEntityFarmland)
                {
                    (be as CANBlockEntityFarmland).WaterFarmland(secondsUsed - prevsecondsused, true);
                }
                else if (be is BlockEntityFarmland)
                {
                    (be as BlockEntityFarmland).WaterFarmland(secondsUsed - prevsecondsused, true);
                }
            }
            float speed = 3f;
            if (world.Side == EnumAppSide.Client)
            {
                ModelTransform tf = new ModelTransform();
                tf.EnsureDefaultValues();
                tf.Origin.Set(0.5f, 0.2f, 0.5f);
                tf.Translation.Set(-Math.Min(0.25f, speed * secondsUsed / 2f), 0f, 0f);
                tf.Rotation.Z = GameMath.Min(new float[]
                {
                    60f,
                    secondsUsed * 90f * speed,
                    120f - remainingwater * 4f
                });
                byEntity.Controls.UsingHeldItemTransformBefore = tf;
            }
            IPlayer byPlayer = null;
            if (byEntity is EntityPlayer)
            {
                byPlayer = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
            }
            if (secondsUsed > 1f / speed)
            {
                Vec3d pos = blockSel.Position.ToVec3d().Add(blockSel.HitPosition);
                if (notOnSolidblock)
                {
                    pos.Y = (double)((int)pos.Y) + 0.05;
                }
                BlockWateringCan.WaterParticles.MinPos = pos.Add(-0.0625, 0.0625, -0.0625);
                byEntity.World.SpawnParticles(BlockWateringCan.WaterParticles, byPlayer);
            }
            __result = true;
            return true;
        }
            //SEEDS
        public static bool Prefix_ItemPlantableSeed_OnHeldInteractStart(Vintagestory.GameContent.ItemPlantableSeed __instance, ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ICoreAPI ___api)
        {
            //(byEntity as EntityPlayer).Player.InventoryManager
            if (blockSel == null)
            {
                return false;
            }

            BlockPos position = blockSel.Position;
            string text = itemslot.Itemstack.Collectible.LastCodePart();
            if (text == "bellpepper")
            {
                return false;
            }

            BlockEntity blockEntity = byEntity.World.BlockAccessor.GetBlockEntity(position);
            if ((blockEntity is CANBlockEntityFarmland))
            {
                if (___api.Side == EnumAppSide.Client)
                {
                    handHandling = EnumHandHandling.PreventDefault;
                    return false;
                }

                AgriPlant agriPlant = cancrops.GetPlants().getPlant(text);

                if (agriPlant == null)
                {
                    return false;
                }

                Block block = byEntity.World.GetBlock((new AssetLocation(agriPlant.Domain + ":crop-" + text + "-1")));
                //"game:crop-" + text + "-1"));

                if (block == null)
                {
                    return false;
                }

                IPlayer player = null;
                if (byEntity is EntityPlayer)
                {
                    player = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
                }

                bool num = ((CANBlockEntityFarmland)blockEntity).TryPlant(block, itemslot.Itemstack, agriPlant);
                if (num)
                {
                    byEntity.World.PlaySoundAt(new AssetLocation("game:sounds/block/plant"), position.X, position.Y, position.Z, player);
                    ((byEntity as EntityPlayer)?.Player as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
                    if (player == null || player.WorldData?.CurrentGameMode != EnumGameMode.Creative)
                    {
                        itemslot.TakeOut(1);
                        itemslot.MarkDirty();
                    }
                }

                if (num)
                {
                    handHandling = EnumHandHandling.PreventDefault;
                }
                return false;
            }
            return true;
        }
        public static void Postfix_ItemPlantableSeed_GetHeldItemInfo(Vintagestory.GameContent.ItemPlantableSeed __instance, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo, ICoreAPI ___api)
        {
            if (inSlot.Itemstack.Attributes.HasAttribute("genome"))
            {
                ITreeAttribute genomeTree = inSlot.Itemstack.Attributes.GetTreeAttribute("genome");
                foreach (var gene in Genome.genes)
                {
                    if (gene.Value)
                    {
                        ITreeAttribute geneTree = genomeTree.GetTreeAttribute(gene.Key);
                        dsc.Append("<font color=\"" + cancrops.config.gene_color_int[gene.Key] + "\">" + Lang.Get("cancrops:" + gene.Key + "-stat") + "</font>");
                        dsc.Append(string.Format(": {0} ", geneTree.GetInt("D")));
                    }
                }
                ITreeAttribute resistanceTree = genomeTree.GetTreeAttribute("resistance");
                if(resistanceTree != null)
                {
                    Block block = world.GetBlock(__instance.CodeWithPath("crop-" + inSlot.Itemstack.Collectible.LastCodePart() + "-1"));
                    if (block != null && block.CropProps != null)
                    {
                        dsc.AppendLine();
                        dsc.Append("(" + "<font color=\"" 
                            + cancrops.config.gene_color_int["resistance-cold"]
                            + "\">"
                            + (block.CropProps.ColdDamageBelow - resistanceTree.GetInt("D") * cancrops.config.coldResistanceByStat) 
                            
                            + "</font>");
                        dsc.Append(", ");
                        dsc.Append("<font color=\""
                            + cancrops.config.gene_color_int["resistance-heat"]
                            + "\">"
                            + (block.CropProps.HeatDamageAbove + resistanceTree.GetInt("D") * cancrops.config.heatResistanceByStat)

                            + "</font>" + ")");
                    }
                }
            }
            
        }



        public static bool Prefix_DoTill(Vintagestory.GameContent.ItemHoe __instance, float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (blockSel == null)
            {
                return false;
            }
            BlockPos pos = blockSel.Position;
            Block block = byEntity.World.BlockAccessor.GetBlock(pos);
            if (!block.Code.Path.StartsWith("soil"))
            {
                return false;
            }
            string fertility = block.LastCodePart(1);
            Block farmland = byEntity.World.GetBlock(new AssetLocation("cancrops:canfarmland-dry-" + fertility));
            IPlayer byPlayer = (byEntity as EntityPlayer).Player;
            if (farmland == null || byPlayer == null)
            {
                return false;
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
            return false;
        }
        public static bool Prefix_GetPlacedBlockInfo(Vintagestory.GameContent.BlockCrop __instance, IWorldAccessor world, BlockPos pos, IPlayer forPlayer, ref string __result)
        {
            Block block = world.BlockAccessor.GetBlock(pos.DownCopy(1));
            if (block is BlockFarmland)
            {
                return true;
            }
            
            if (block is CANBlockFarmland)
            {
                __result = block.GetPlacedBlockInfo(world, pos.DownCopy(1), forPlayer);
                return false;
            }
            __result = Lang.Get("Required Nutrient: {0}", new object[]
            {
                __instance.CropProps.RequiredNutrient
            }) + "\n" + Lang.Get("Growth Stage: {0} / {1}", new object[]
            {
                __instance.CurrentStage(),
                __instance.CropProps.GrowthStages
            });
            return false;
        }
        public static AssetLocation Stub_RegistryObject_CodeWithPath(object instance, string path)
        {
            // its a stub so it has no initial content
            throw new NotImplementedException("It's a stub");
            // (instance as Block).OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        }
        public static void Stub_Item_GetHeldItemInfo(object instance, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            // its a stub so it has no initial content
            throw new NotImplementedException("It's a stub");
            // (instance as Block).OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        }
        public static bool Stub_Block_OnBlockInteractStart(object instance, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            // its a stub so it has no initial content
            throw new NotImplementedException("It's a stub");
            // (instance as Block).OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        }
        

        public static bool Prefix_BlockFarmland_OnBlockInteractStart(Vintagestory.GameContent.BlockFarmland __instance, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref bool __result)
        {
            BlockEntityFarmland befarmland = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityFarmland;
            if (befarmland != null)
            {
                __result = befarmland.OnBlockInteract(byPlayer) || Stub_Block_OnBlockInteractStart(__instance, world, byPlayer, blockSel);
                return false;
            }
            CANBlockEntityFarmland canbefarmland = world.BlockAccessor.GetBlockEntity(blockSel.Position) as CANBlockEntityFarmland;
            __result = (canbefarmland != null && canbefarmland.OnBlockInteract(byPlayer)) || Stub_Block_OnBlockInteractStart(__instance, world, byPlayer, blockSel);
            return false;
        }
        public static IEnumerable<CodeInstruction> Transpiler_BlockFarmland_OnBlockInteractStart(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            bool found = false;
            bool found2 = false;
            var proxyMethod = AccessTools.Method(typeof(CANBlockEntityFarmland), "OnBlockInteract");
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (!found &&
                    codes[i].opcode == OpCodes.Isinst && codes[i + 1].opcode == OpCodes.Stloc_0 && codes[i + 2].opcode == OpCodes.Ldloc_0 && codes[i - 1].opcode == OpCodes.Callvirt)
                {
                    //yield return new CodeInstruction(OpCodes.Castclass, typeof(CANBlockEntityFarmland));
                    yield return new CodeInstruction(OpCodes.Castclass, typeof(CANBlockEntityFarmland));
                    yield return new CodeInstruction(OpCodes.Isinst, typeof(CANBlockEntityFarmland));

                    yield return new CodeInstruction(OpCodes.Stloc_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    //yield return new CodeInstruction(OpCodes.Castclass, typeof(CANBlockEntityFarmland));
                    i += 2;
                    found = true;
                    continue;
                }

                if (!found2 &&
                   codes[i].opcode == OpCodes.Callvirt && codes[i + 1].opcode == OpCodes.Brfalse_S && codes[i + 2].opcode == OpCodes.Ldc_I4_1 && codes[i - 1].opcode == OpCodes.Ldarg_2
                   && codes[i - 2].opcode == OpCodes.Ldloc_0)
                {
                    //yield return codes[i];
                    //yield return new CodeInstruction(OpCodes.Castclass, typeof(CANBlockEntityFarmland));
                    //yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Callvirt, proxyMethod);
                    found2 = true;
                    //i += 2;
                    continue;
                }
                yield return codes[i];
            }
        }
        
        public static ItemStack[] Stub_GetDrops(object instance, IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
        {
            // its a stub so it has no initial content
            throw new NotImplementedException("It's a stub");
            // (instance as Block).OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        }
        public static bool Prefix_GetDrops(Vintagestory.GameContent.BlockCrop __instance, IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref ItemStack[] __result, float dropQuantityMultiplier = 1f)
        {
            CANBlockEntityFarmland canbefarmland = world.BlockAccessor.GetBlockEntity(pos.DownCopy(1)) as CANBlockEntityFarmland;
            if (canbefarmland != null)
            {
                __instance.SplitDropStacks = false;

                ItemStack[] candrops = Stub_GetDrops(__instance, world, pos, byPlayer, dropQuantityMultiplier);
                if (canbefarmland == null)
                {
                    List<ItemStack> moddrops = new List<ItemStack>();
                    foreach (ItemStack drop in candrops)
                    {
                        if (!(drop.Item is ItemPlantableSeed))
                        {
                            drop.StackSize = GameMath.RoundRandom(world.Rand, BlockCrop.WildCropDropMul * (float)drop.StackSize);
                        }
                        if (drop.StackSize > 0)
                        {
                            moddrops.Add(drop);
                        }
                    }
                    candrops = moddrops.ToArray();
                }
                if (canbefarmland != null)
                {
                    candrops = canbefarmland.GetDrops(candrops, byPlayer);
                }
                __result = candrops;
                return false;
            }
            else
            {
                BlockEntityFarmland befarmland2 = world.BlockAccessor.GetBlockEntity(pos.DownCopy(1)) as BlockEntityFarmland;

                __instance.SplitDropStacks = false;
                ItemStack[] drops = Stub_GetDrops(__instance, world, pos, byPlayer, dropQuantityMultiplier);
                if (befarmland2 == null)
                {
                    List<ItemStack> moddrops = new List<ItemStack>();
                    foreach (ItemStack drop in drops)
                    {
                        if (!(drop.Item is ItemPlantableSeed))
                        {
                            drop.StackSize = GameMath.RoundRandom(world.Rand, BlockCrop.WildCropDropMul * (float)drop.StackSize);
                        }
                        if (drop.StackSize > 0)
                        {
                            moddrops.Add(drop);
                        }
                    }
                    drops = moddrops.ToArray();
                }
                if (befarmland2 != null)
                {
                    drops = befarmland2.GetDrops(drops);
                }
                __result = drops;
                return false;
            }














           /* BlockEntityFarmland befarmland = world.BlockAccessor.GetBlockEntity(pos.DownCopy(1)) as BlockEntityFarmland;
            if (befarmland == null)
            {
                CANBlockEntityFarmland canbefarmland = world.BlockAccessor.GetBlockEntity(pos.DownCopy(1)) as CANBlockEntityFarmland;
                if (canbefarmland == null)
                {
                    dropQuantityMultiplier *= ((byPlayer != null) ? byPlayer.Entity.Stats.GetBlended("wildCropDropRate") : 1f);
                }
                __instance.SplitDropStacks = false;
                
                ItemStack[] candrops = Stub_GetDrops(__instance, world, pos, byPlayer, dropQuantityMultiplier);
                if (canbefarmland == null)
                {
                    List<ItemStack> moddrops = new List<ItemStack>();
                    foreach (ItemStack drop in candrops)
                    {
                        if (!(drop.Item is ItemPlantableSeed))
                        {
                            drop.StackSize = GameMath.RoundRandom(world.Rand, BlockCrop.WildCropDropMul * (float)drop.StackSize);
                        }
                        if (drop.StackSize > 0)
                        {
                            moddrops.Add(drop);
                        }
                    }
                    candrops = moddrops.ToArray();
                }
                if (canbefarmland != null)
                {
                    candrops = canbefarmland.GetDrops(candrops, byPlayer);
                }
                __result = candrops;
                return false;
            }
            __instance.SplitDropStacks = false;
            ItemStack[] drops = Stub_GetDrops(__instance, world, pos, byPlayer, dropQuantityMultiplier);
            if (befarmland == null)
            {
                List<ItemStack> moddrops = new List<ItemStack>();
                foreach (ItemStack drop in drops)
                {
                    if (!(drop.Item is ItemPlantableSeed))
                    {
                        drop.StackSize = GameMath.RoundRandom(world.Rand, BlockCrop.WildCropDropMul * (float)drop.StackSize);
                    }
                    if (drop.StackSize > 0)
                    {
                        moddrops.Add(drop);
                    }
                }
                drops = moddrops.ToArray();
            }
            if (befarmland != null)
            {
                drops = befarmland.GetDrops(drops);
            }
            __result = drops;
            return false;*/
        }


        public static IEnumerable<CodeInstruction> Transpiler_GetDrops(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            bool found = false;
            bool found2 = false;
            var proxyMethod = AccessTools.Method(typeof(CANBlockEntityFarmland), "GetDrops");
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (!found &&
                    codes[i].opcode == OpCodes.Isinst && codes[i + 1].opcode == OpCodes.Stloc_0 && codes[i + 2].opcode == OpCodes.Ldloc_0 && codes[i - 1].opcode == OpCodes.Callvirt)
                {
                    //yield return new CodeInstruction(OpCodes.Castclass, typeof(CANBlockEntityFarmland));
                    yield return new CodeInstruction(OpCodes.Castclass, typeof(CANBlockEntityFarmland));
                    yield return new CodeInstruction(OpCodes.Isinst, typeof(CANBlockEntityFarmland));
                    
                    yield return new CodeInstruction(OpCodes.Stloc_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    //yield return new CodeInstruction(OpCodes.Castclass, typeof(CANBlockEntityFarmland));
                    i += 2;
                    found = true;
                    continue;
                }

                if (!found2 &&
                   codes[i].opcode == OpCodes.Ldloc_0 && codes[i + 1].opcode == OpCodes.Ldloc_1 && codes[i + 2].opcode == OpCodes.Callvirt && codes[i - 1].opcode == OpCodes.Brfalse_S
                   && codes[i - 2].opcode == OpCodes.Ldloc_0)
                {
                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Castclass, typeof(CANBlockEntityFarmland));
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Callvirt, proxyMethod);
                    found2 = true;
                    i += 2;
                    continue;
                }
                yield return codes[i];
            }
        }
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Block), "OnBlockBroken")]
        public static void Stub_Block_OnBlockBroken(object instance, IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
        {
            // its a stub so it has no initial content
            throw new NotImplementedException("It's a stub");
           // (instance as Block).OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        }
        //Prefix_OnBlockInteractStart
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Vintagestory.GameContent.BlockCrop), "OnBlockBroken")]
        public static bool Prefix_OnBlockBroken(Vintagestory.GameContent.BlockCrop __instance, IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
        {
            Stub_Block_OnBlockBroken(__instance, world, pos, byPlayer, dropQuantityMultiplier);
            BlockEntity blockEntityFarmland = world.BlockAccessor.GetBlockEntity(pos.DownCopy(1));
            if (blockEntityFarmland is CANBlockEntityFarmland canBe)
            {
                canBe.OnCropBlockBroken(byPlayer);
                return false;
            }
            
            
            return false;
        }
        

        public static MethodInfo OnCropBlockBrokenMethod = typeof(CANBlockEntityFarmland).GetMethod("OnCropBlockBroken");
        public static IEnumerable<CodeInstruction> Transpiler_OnBlockBroken(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            bool found = false;
            bool found2 = false;
            var proxyMethod = AccessTools.Method(typeof(CANBlockEntityFarmland), "OnCropBlockBroken");
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (!found &&
                    codes[i].opcode == OpCodes.Isinst && codes[i + 1].opcode == OpCodes.Dup && codes[i + 2].opcode == OpCodes.Brtrue_S && codes[i - 1].opcode == OpCodes.Callvirt)
                {
                    
                    //yield return new CodeInstruction(OpCodes.Castclass, typeof(CANBlockEntityFarmland));
                    yield return new CodeInstruction(OpCodes.Isinst, typeof(CANBlockEntityFarmland));

                    found = true;
                    continue;
                }

                if (!found2 &&
                   codes[i].opcode == OpCodes.Call && codes[i + 1].opcode == OpCodes.Ret && codes[i - 3].opcode == OpCodes.Brtrue_S && codes[i - 1].opcode == OpCodes.Ret
                   && codes[i - 2].opcode == OpCodes.Pop)
                {
                    yield return new CodeInstruction(OpCodes.Castclass, typeof(CANBlockEntityFarmland));
                    yield return new CodeInstruction(OpCodes.Call, proxyMethod);
                    found2 = true;
                    continue;
                }
                yield return codes[i];
            }
        }
        public static IEnumerable<CodeInstruction> Transpiler_OnLoaded(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            bool found = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (!found &&
                    codes[i].opcode == OpCodes.Ldtoken && codes[i + 1].opcode == OpCodes.Call && codes[i + 2].opcode == OpCodes.Call && codes[i - 1].opcode == OpCodes.Callvirt)
                {
                    //yield return new CodeInstruction(OpCodes.Castclass, typeof(CANBlockEntityFarmland));
                    yield return new CodeInstruction(OpCodes.Ldtoken, typeof(CANBlockEntityFarmland));

                    //found = true;
                    continue;
                }
                yield return codes[i];
            }
        }
        public static IEnumerable<CodeInstruction> Transpiler_OnHeldInteractStart(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            bool found = false;
            bool found2 = false;
            var proxyMethod = AccessTools.Method(typeof(CANBlockEntityFarmland), "TryPlant");
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (!found &&
                    codes[i].opcode == OpCodes.Isinst && codes[i + 1].opcode == OpCodes.Brfalse && codes[i + 2].opcode == OpCodes.Ldarg_2 && codes[i - 1].opcode == OpCodes.Ldloc_2)
                {
                    //yield return new CodeInstruction(OpCodes.Castclass, typeof(CANBlockEntityFarmland));
                    yield return new CodeInstruction(OpCodes.Isinst, typeof(CANBlockEntityFarmland));

                    found = true;
                    continue;
                }

                if (!found2 &&
                   codes[i].opcode == OpCodes.Castclass && codes[i + 1].opcode == OpCodes.Ldloc_3 && codes[i + 2].opcode == OpCodes.Callvirt && codes[i - 1].opcode == OpCodes.Ldloc_2
                   && codes[i - 2].opcode == OpCodes.Stloc_S)
                {
                    yield return new CodeInstruction(OpCodes.Castclass, typeof(CANBlockEntityFarmland));
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Callvirt, proxyMethod);
                    found2 = true;
                    i += 2;
                    continue;
                }
                yield return codes[i];
            }
        }
       /* [HarmonyReversePatch]
        [HarmonyPatch(typeof(BlockEntityFarmland), "OnBlockInteract")]
        public static bool Stub_BlockEntityFarmland_OnBlockInteract(object instance, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            // its a stub so it has no initial content
            throw new NotImplementedException("It's a stub");
        }
        public static void Prefix_OnBlockInteractStart(Vintagestory.GameContent.BlockCrop __instance, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref bool __result)
        {
            BlockEntityFarmland blockEntityFarmland = world.BlockAccessor.GetBlockEntity(blockSel.Position.DownCopy()) as BlockEntityFarmland;
            if (blockEntityFarmland != null && blockEntityFarmland.OnBlockInteract(byPlayer))
            {
                __result = true;
                return;
            }

            __result = Stub_BlockEntityFarmland_OnBlockInteract(__instance, world, byPlayer, blockSel);
            return;
        }*/
        public static MethodInfo OnBlockInteractMethod = typeof(CANBlockEntityFarmland).GetMethod("OnBlockInteract");
        public static IEnumerable<CodeInstruction> Transpiler_OnBlockInteractStart(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            bool found = false;
            bool found2 = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (!found &&
                    codes[i].opcode == OpCodes.Isinst && codes[i + 1].opcode == OpCodes.Stloc_0 && codes[i + 2].opcode == OpCodes.Ldloc_0 && codes[i - 1].opcode == OpCodes.Callvirt)
                {
                    //yield return new CodeInstruction(OpCodes.Castclass, typeof(CANBlockEntityFarmland));
                    yield return new CodeInstruction(OpCodes.Isinst, typeof(CANBlockEntityFarmland));
                    
                    found = true;
                    continue;
                }

                if (!found2 &&
                   codes[i].opcode == OpCodes.Callvirt && codes[i + 1].opcode == OpCodes.Brfalse_S && codes[i + 2].opcode == OpCodes.Ldc_I4_1 && codes[i - 1].opcode == OpCodes.Ldarg_2
                   && codes[i - 2].opcode == OpCodes.Ldloc_0)
                {
                    yield return new CodeInstruction(OpCodes.Callvirt, OnBlockInteractMethod);
                    found2 = true;
                    continue;
                }
                yield return codes[i];
            }
        }
        public static void OnCreatedFromSoil(CANBlockEntityFarmland be, Block bl)
        {
            //var c = 3;
            (be as CANBlockEntityFarmland).OnCreatedFromSoil(bl);
        }
        public static IEnumerable<CodeInstruction> Transpiler_ItemHoe_DoTill(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            bool found = false;
            bool found2 = false;
            var proxyMethod = AccessTools.Method(typeof(harmPatch), "OnCreatedFromSoil");
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (!found &&
                    codes[i].opcode == OpCodes.Ldstr && codes[i + 1].opcode == OpCodes.Ldloc_2 && codes[i + 2].opcode == OpCodes.Call && codes[i - 1].opcode == OpCodes.Ldfld)
                {
                    //yield return new CodeInstruction(OpCodes.Ldstr, "cancrops:canfarmland-dry-");
                    //yield return new CodeInstruction(OpCodes.Call, proxyMethod);
                    found = true;
                    //continue;
                }

                if (!found2 &&
                   codes[i].opcode == OpCodes.Ldloc_S && codes[i + 1].opcode == OpCodes.Isinst && codes[i + 2].opcode == OpCodes.Brfalse_S && codes[i - 1].opcode == OpCodes.Stloc_S
                   && codes[i - 2].opcode == OpCodes.Callvirt)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                    yield return new CodeInstruction(OpCodes.Castclass, typeof(CANBlockEntityFarmland));
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 1);
                    yield return new CodeInstruction(OpCodes.Call, proxyMethod);
                    found2 = true;
                }
                yield return codes[i];
            }
        }
    }
}
