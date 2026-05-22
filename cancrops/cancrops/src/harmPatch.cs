using cancrops.src.BE;
using cancrops.src.genetics;
using cancrops.src.genetics.abstractions;
using cancrops.src.genetics.genes;
using cancrops.src.implementations;
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

            int gain = beCrop.Genome?.Gain?.Dominant?.Value ?? 0;
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
                    int growthValue = beCrop.Genome.Growth?.Dominant?.Value ?? 0;
                    __result =(double)(__instance.Api.World.Calendar.HoursPerDay * totalDays
                        / (float)block.CropProps.GrowthStages
                        * (1f / __instance.GetGrowthRate(block.CropProps.RequiredNutrient))
                        * (float)(0.9 + 0.2 * CANBECrop.rand.NextDouble())
                        / ___growthRateMul)
                            * (1f - (growthValue * 0.05));
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
                int resistance = beCrop.Genome?.Resistance?.Dominant?.Value ?? 0;
                if (resistance > 0)
                {
                    return cancrops.config.coldResistanceByStat * resistance;
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
                int resistance = beCrop.Genome?.Resistance?.Dominant?.Value ?? 0;
                if (resistance > 0)
                {
                    return cancrops.config.heatResistanceByStat * resistance;
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
            if (!inSlot.Itemstack.Attributes.HasAttribute("genome")) return;

            // Use the Genome facade so old-format NBT migrates and species pair (with recessive
            // allele) is visible. ResolvePlantFromSeedStack inside provides the fallback species
            // when reading legacy items.
            Genome genome = CommonUtils.GetSeedGenomeFromAttribute(inSlot.Itemstack);
            if (genome == null) return;

            // Stats line(s)
            foreach (var kv in Genome.genes)
            {
                if (kv.Value) continue; // hidden
                var snap = genome.GetGeneByName(kv.Key);
                if (snap == null) continue;
                cancrops.config.gene_color_int.TryGetValue(kv.Key, out string color);
                dsc.Append("<font color=\"" + (color ?? "white") + "\">" + Lang.Get("cancrops:" + kv.Key + "-stat") + "</font>");
                dsc.AppendFormat(": {0} ", snap.Dominant.Value);
            }

            // Species inheritance: show "Wheat (carrying rye)" when heterozygous.
            AppendSpeciesLine(genome, dsc);

            // Effective light range: MinLight/MaxLight extended by LightToleranceFactor * strength.
            var speciesGeneForLight = GeneRegistry.Get<AgriPlant>(SpeciesGene.GENE_ID);
            var speciesPair = speciesGeneForLight != null ? genome.GetGenePair(speciesGeneForLight) : null;
            AgriPlant plantForLight = speciesPair?.Dominant?.Trait;
            if (plantForLight?.Requirement != null)
            {
                int strengthVal = genome.Strength?.Dominant?.Value ?? 0;
                int bonus = (int)(plantForLight.Requirement.LightToleranceFactor * strengthVal);
                dsc.AppendLine();
                dsc.Append(Lang.Get("cancrops:light-range") + ": " + plantForLight.Requirement.MinLight + "–" + plantForLight.Requirement.MaxLight + " (±" + bonus + ")");
            }

            // Cold/heat resistance bracket from resistance stat.
            var resistance = genome.Resistance;
            if (resistance != null)
            {
                Block block = world.GetBlock(__instance.CodeWithPath("crop-" + inSlot.Itemstack.Collectible.LastCodePart() + "-1"));
                if (block != null && block.CropProps != null)
                {
                    cancrops.config.gene_color_int.TryGetValue("resistance-cold", out string coldColor);
                    cancrops.config.gene_color_int.TryGetValue("resistance-heat", out string heatColor);
                    dsc.AppendLine();
                    dsc.Append("(<font color=\"" + (coldColor ?? "white") + "\">"
                        + (block.CropProps.ColdDamageBelow - resistance.Dominant.Value * cancrops.config.coldResistanceByStat)
                        + "</font>, <font color=\"" + (heatColor ?? "white") + "\">"
                        + (block.CropProps.HeatDamageAbove + resistance.Dominant.Value * cancrops.config.heatResistanceByStat)
                        + "</font>)");
                }
            }
        }

        // Reads the species pair via the GeneRegistry and appends a line to the tooltip.
        // Format: "Species: Wheat" for homozygous, "Species: Wheat (carrying rye)" for heterozygous.
        private static void AppendSpeciesLine(Genome genome, StringBuilder dsc)
        {
            var speciesGene = GeneRegistry.Get<AgriPlant>(SpeciesGene.GENE_ID);
            if (speciesGene == null) return;
            var pair = genome.GetGenePair(speciesGene);
            if (pair == null) return;
            AgriPlant dom = pair.Dominant?.Trait;
            AgriPlant rec = pair.Recessive?.Trait;
            if (dom == null) return;

            string domLabel = PlantDisplayLabel(dom);
            cancrops.config.gene_color_int.TryGetValue("species", out string color);
            string label = Lang.Get("cancrops:species-stat");
            dsc.AppendLine();
            dsc.Append("<font color=\"" + (color ?? "white") + "\">" + label + "</font>: " + domLabel);
            if (rec != null && !PlantsEqual(dom, rec))
            {
                dsc.Append(" <font color=\"#888888\">(carrying " + PlantDisplayLabel(rec) + ")</font>");
            }
        }

        private static string PlantDisplayLabel(AgriPlant plant)
        {
            if (plant == null) return "?";
            string key = "cancrops:plant-" + plant.Id;
            string localised = Lang.Get(key);
            // Lang.Get returns the key itself when no translation exists — fall back to the id.
            return localised == key ? plant.Id : localised;
        }

        private static bool PlantsEqual(AgriPlant a, AgriPlant b)
        {
            return a?.Domain == b?.Domain && a?.Id == b?.Id;
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
