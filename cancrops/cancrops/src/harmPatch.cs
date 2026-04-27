using cancrops.src.BE;
using cancrops.src.genetics;
using cancrops.src.utility;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace cancrops.src
{
    [HarmonyPatch]
    public class harmPatch
    {
        public static bool Prefix_ItemPlantableSeed_OnCreatedByCrafting(CollectibleObject __instance, ItemSlot[] allInputSlots, ItemSlot outputSlot, GridRecipe byRecipe)
        {
            if(__instance is not ItemPlantableSeed)
            {
                return true;
            }
            bool skip = true;
            var slots = new List<Genome>();
            foreach (var it in allInputSlots)
            {
                if (!it.Empty && it.Itemstack.Attributes != null && it.Itemstack.Attributes.HasAttribute("genome"))
                {
                    slots.Add(CommonUtils.GetSeedGenomeFromAttribute(it.Itemstack));
                }
            }
            if(slots.Count < 2)
            {
                return skip;
            }
            CommonUtils.MergeGenomes(slots, out Genome genome);
            CommonUtils.ApplyGenomeTreeToItemstack(genome, outputSlot.Itemstack);
            return skip;
        }
        public static void Prefix_GetPlacedBlockInfo_New(Vintagestory.GameContent.BlockCrop __instance, IWorldAccessor world, BlockPos pos, IPlayer forPlayer, ref string __result)
        {
            if (world.BlockAccessor.GetBlockEntity<CANBECrop>(pos) is CANBECrop beCrop)
            {
                StringBuilder sb = new();
                foreach (var gene in Genome.genes)
                {
                    if (gene.Value)
                    {
                        sb.Append(gene.Key + " " + gene.Value);
                    }
                }
            }
        }
        public static void Prefix_BlockEntityFarmland_GetDrops(Vintagestory.GameContent.BlockEntityFarmland __instance, ItemStack[] drops, ref ItemStack[] __result)
        {
            List<ItemStack> newDrops = RemoveDefaultSeeds(drops);
            if(!(__instance.Api.World.BlockAccessor.GetBlockEntity<CANBECrop>(__instance.Pos.UpCopy()) is CANBECrop beCrop))
            {
                return;
            }
            if (beCrop.agriPlant == null)
            {
                return;
            }
            if (CANBECrop.rand.NextDouble() < (beCrop.agriPlant.SeedDropChance + beCrop.agriPlant.SeedDropBonus /** GetCropStage(this.Block)*/))
            {
                var seed = CommonUtils.GetSeedItemStackFromFarmland(beCrop.Genome, beCrop.agriPlant);
                newDrops.Add(seed);
            }

            int gain = beCrop.Genome.Gain.Dominant.Value;
            foreach (var it in newDrops)
            {
                if (it.Item is ItemPlantableSeed)
                {
                    Block block = beCrop.GetCrop();
                    int stage = 0;
                    if (block != null)
                    {
                        stage = beCrop.GetCropStage(block);
                    }

                    it.StackSize = Math.Min(2, (int)(beCrop.agriPlant.SeedDropChance + beCrop.agriPlant.SeedDropBonus * stage));
                    continue;
                }
                it.StackSize += (int)((gain * CANBECrop.rand.Next(1, 3) * 0.2) * it.StackSize);
            }
            ApplyStrengthBuff(newDrops, __instance.Api);
            __result = newDrops.ToArray();
            //drops = newDrops.ToArray();
        }
        public static bool Prefix_BlockEntityFarmland_GetHoursForNextStage(Vintagestory.GameContent.BlockEntityFarmland __instance, float ___growthRateMul, ref double __result)
        {
            if (__instance.Api.World.BlockAccessor.GetBlockEntity<CANBECrop>(__instance.Pos.UpCopy()) is CANBECrop beCrop)
            {
                Block block = beCrop.GetCrop();
                if (block == null)
                {
                    return true;
                }
                float totalDays = block.CropProps.TotalGrowthDays;
                if (totalDays > 0f)
                {
                    totalDays = totalDays / 12f * (float)__instance.Api.World.Calendar.DaysPerMonth;
                }
                else
                {
                    totalDays = block.CropProps.TotalGrowthMonths * (float)__instance.Api.World.Calendar.DaysPerMonth;
                }
                if (beCrop.Genome != null)
                {
                    __result =(double)(__instance.Api.World.Calendar.HoursPerDay * totalDays
                        / (float)block.CropProps.GrowthStages
                        * (1f / __instance.GetGrowthRate(block.CropProps.RequiredNutrient))
                        * (float)(0.9 + 0.2 * CANBECrop.rand.NextDouble())
                        / ___growthRateMul)
                            * (1f - (beCrop.Genome.Growth.Dominant.Value * 0.05));
                    return false;
                }
            }
            return true;
        }
        public static bool Prefix_BlockCrop_OnBlockInteractStart_New(Vintagestory.GameContent.BlockCrop __instance, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref bool __result)
        {
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is CANBECrop beCrop && beCrop.TryClipPlant(byPlayer))
            {
                __result = true;
                return false;
            }
            return true;
        }
        public static float GetColdResistance(Vintagestory.GameContent.BlockEntityFarmland farmland)
        {
            if (farmland.Api.World.BlockAccessor.GetBlockEntity<CANBECrop>(farmland.Pos.UpCopy()) is CANBECrop beCrop)
            {
                if (beCrop.Genome != null)
                {
                    return cancrops.config.coldResistanceByStat * beCrop.Genome.Resistance.Dominant.Value;
                }
            }
            return 1f;
        }
        public static IEnumerable<CodeInstruction> Transpiler_BlockEntityFarmland_Update_Cold(IEnumerable<CodeInstruction> instructions)
        {
            bool found = false;
            var codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                if (!found &&
                    codes[i].opcode == OpCodes.Ldfld && codes[i + 1].opcode == OpCodes.Bge_Un_S && codes[i + 2].opcode == OpCodes.Ldloc_S && codes[i - 1].opcode == OpCodes.Ldfld)
                {
                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    var method = AccessTools.Method(typeof(harmPatch), nameof(harmPatch.GetColdResistance));
                    yield return new CodeInstruction(OpCodes.Call, method);
                    yield return new CodeInstruction(OpCodes.Sub);
                    found = true;
                    continue;
                }
                yield return codes[i];
            }
        }
        public static float GetHeatResistance(Vintagestory.GameContent.BlockEntityFarmland farmland)
        {
            if (farmland.Api.World.BlockAccessor.GetBlockEntity<CANBECrop>(farmland.Pos.UpCopy()) is CANBECrop beCrop)
            {
                if (beCrop.Genome != null)
                {
                    /*var field = typeof(BlockEntityFarmland).GetField(
                        "totalHoursLastUpdate",
                        BindingFlags.Instance | BindingFlags.NonPublic
                    );
                    double c2 = (double)(field.GetValue(farmland));
                    var conds = farmland.Api.World.BlockAccessor.GetClimateAt(farmland.Pos, 
                        EnumGetClimateMode.ForSuppliedDate_TemperatureRainfallOnly, c2 / farmland.Api.World.Calendar.HoursPerDay);
                    if(conds.Temperature > 40)
                    {
                        var c = 3;
                    }*/
                    return cancrops.config.heatResistanceByStat * beCrop.Genome.Resistance.Dominant.Value;
                }
            }
            return 1f;
        }
        public static double AdjustLightFactor(double original, BlockEntityFastForwardGrowth ffg)
        {
            if (original >= 1.0) return original;
            if (ffg.Api.World.BlockAccessor.GetBlockEntity<CANBECrop>(ffg.Pos.UpCopy()) is CANBECrop be
                && be.agriPlant != null)
            {
                return 1.0 - (1.0 - original) * be.agriPlant.LightSensitivity;
            }
            return original;
        }
        public static IEnumerable<CodeInstruction> Transpiler_BlockEntityFastForwardGrowth_Update_Light(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            bool found = false;
            var helper = AccessTools.Method(typeof(harmPatch), nameof(harmPatch.AdjustLightFactor));

            for (int i = 0; i < codes.Count; i++)
            {
                if (!found
                    && i + 2 < codes.Count
                    && codes[i].opcode == OpCodes.Call
                    && codes[i].operand is MethodInfo mi
                    && mi.Name == "Clamp"
                    && mi.DeclaringType == typeof(Vintagestory.API.MathTools.GameMath)
                    && codes[i + 1].opcode == OpCodes.Conv_R8
                    && codes[i + 2].opcode == OpCodes.Stloc_S)
                {
                    yield return codes[i];
                    yield return codes[i + 1];
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, helper);
                    yield return codes[i + 2];
                    i += 2;
                    found = true;
                    continue;
                }
                yield return codes[i];
            }
        }
        public static IEnumerable<CodeInstruction> Transpiler_BlockEntityFarmland_Update_Heat(IEnumerable<CodeInstruction> instructions)
        {
            bool found = false;
            var codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                if (!found &&
                    codes[i].opcode == OpCodes.Ldfld && codes[i + 1].opcode == OpCodes.Cgt && codes[i + 2].opcode == OpCodes.Br_S && codes[i - 1].opcode == OpCodes.Ldfld)
                {
                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    var method = AccessTools.Method(typeof(harmPatch), nameof(harmPatch.GetHeatResistance));
                    yield return new CodeInstruction(OpCodes.Call, method);
                    yield return new CodeInstruction(OpCodes.Add);
                    found = true;
                    continue;
                }
                yield return codes[i];
            }
        }
        ////HELPERS
        public static List<ItemStack> RemoveDefaultSeeds(ItemStack[] drops)
        {

            List<ItemStack> li = new List<ItemStack>();
            if (drops == null)
            {
                return li;
            }
            foreach (var it in drops)
            {
                if (!(it.Item is ItemPlantableSeed))
                {
                    li.Add(it);
                }
            }
            return li;
        }
        private static void ApplyStrengthBuff(List<ItemStack> drops, ICoreAPI api)
        {
            foreach (var it in drops)
            {
                float[] freshHours;
                float[] transitionHours;
                float[] transitionedHours;
                TransitionableProperties[] propsm = it.Collectible.GetTransitionableProperties(api.World, it, null);
                ITreeAttribute attr = new TreeAttribute();
                if (propsm != null)
                    if (!it.Attributes.HasAttribute("createdTotalHours"))
                    {
                        attr.SetDouble("createdTotalHours", api.World.Calendar.TotalHours);
                        attr.SetDouble("lastUpdatedTotalHours", api.World.Calendar.TotalHours);
                        freshHours = new float[propsm.Length];
                        transitionHours = new float[propsm.Length];
                        transitionedHours = new float[propsm.Length];
                        for (int i = 0; i < propsm.Length; i++)
                        {
                            transitionedHours[i] = 0f;
                            freshHours[i] = propsm[i].FreshHours.nextFloat(1f, api.World.Rand) * (1 + cancrops.config.strengthFreshHoursPercentBonus);
                            transitionHours[i] = propsm[i].TransitionHours.nextFloat(1f, api.World.Rand);
                        }
                        attr["freshHours"] = new FloatArrayAttribute(freshHours);
                        attr["transitionHours"] = new FloatArrayAttribute(transitionHours);
                        attr["transitionedHours"] = new FloatArrayAttribute(transitionedHours);
                        it.Attributes["transitionstate"] = attr;
                    }
            }
        }

        /// OLD
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////SEEEDS/////////////////////////////////////////////////////////////////////////////////
        public static void Postfix_ItemPlantableSeed_GetHeldItemInfo(Vintagestory.GameContent.ItemPlantableSeed __instance, ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo, ICoreAPI ___api)
        {
            if (inSlot.Itemstack.Attributes.HasAttribute("genome"))
            {
                ITreeAttribute genomeTree = inSlot.Itemstack.Attributes.GetTreeAttribute("genome");
                foreach (var gene in Genome.genes)
                {
                    ITreeAttribute geneTree = genomeTree.GetTreeAttribute(gene.Key);
                    if(cancrops.config.hidden_genes.TryGetValue(gene.Key, out bool isHidden))
                    {
                        if (isHidden)
                        {
                            continue;
                        }
                    }
                    dsc.Append("<font color=\"" + cancrops.config.gene_color_int[gene.Key] + "\">" + Lang.Get("cancrops:" + gene.Key + "-stat") + "</font>");
                    dsc.Append(string.Format(": {0} ", geneTree.GetInt("D")));              
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
        public static IEnumerable<CodeInstruction> Transpiler_BlockEntityFarmland_Update(IEnumerable<CodeInstruction> instructions)
        {
            bool found = false;
            var codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                if (!found &&
                    codes[i].opcode == OpCodes.Conv_R8 && codes[i + 1].opcode == OpCodes.Stloc_S && codes[i + 2].opcode == OpCodes.Ldarg_0 && codes[i - 1].opcode == OpCodes.Call)
                {
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    found = true;
                    continue;
                }
                yield return codes[i];
            }
        }
    }
}
