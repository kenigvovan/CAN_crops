using System;
using System.Collections.Generic;
using System.Reflection;
using cancrops.src.BE;
using cancrops.src.blocks;
using cancrops.src.commands;
using cancrops.src.cropBehaviors;
using cancrops.src.genetics;
using cancrops.src.genetics.abstractions;
using cancrops.src.genetics.conditions;
using cancrops.src.genetics.conditions.types;
using cancrops.src.genetics.genes;
using cancrops.src.implementations;
using ClimateRangeCondition = global::cancrops.src.genetics.conditions.types.ClimateCondition;
using cancrops.src.items;
using cancrops.src.templates;
using cancrops.src.utility;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.CommandAbbr;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace cancrops.src
{
    public class cancrops: ModSystem
    {
        public static Harmony harmonyInstance;
        public const string harmonyID = "cancrops.Patches";
        public static ICoreServerAPI sapi;
        public static Config config;
        private static AgriPlants agriPlants;
        private static AgriMutations agriMutations;
        private static AgriMutationHandler agriMutationHandler;
        public static ICoreAPI api;
        internal static IServerNetworkChannel serverChannel;
        internal static IClientNetworkChannel clientChannel;
        public override void StartPre(ICoreAPI api)
        {
            base.StartPre(api);
            cancrops.api = api;
        }
        public override void Start(ICoreAPI api)
        {
            //Environment.SetEnvironmentVariable("CAIRO_DEBUG_DISPOSE", "1");
            base.Start(api);
            //Items
            api.RegisterItemClass("CANItemHandCultivator", typeof(CANItemHandCultivator));
            api.RegisterItemClass("CANItemAntiWeed", typeof(CANItemAntiWeed));

            //Blocks
            api.RegisterBlockClass("CANBlockSelectionSticks", typeof(CANBlockSelectionSticks));

            //BE
            api.RegisterBlockEntityClass("CANBECrop", typeof(CANBECrop));
            api.RegisterBlockEntityClass("CANBECrossSticks", typeof(CANBECrossSticks));

            //CROP BEHAVIOUR
            api.RegisterCropBehavior("AgriPlantCropBehavior", typeof(AgriPlantCropBehavior));

            //Patches
            harmonyInstance = new Harmony(harmonyID);

            harmonyInstance.Patch(typeof(Vintagestory.GameContent.BlockEntityFarmland).GetMethod("GetDrops"), postfix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_BlockEntityFarmland_GetDrops")));
            harmonyInstance.Patch(typeof(Vintagestory.GameContent.BlockEntityFarmland).GetMethod("GetHoursForNextStage"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_BlockEntityFarmland_GetHoursForNextStage")));
            harmonyInstance.Patch(typeof(Vintagestory.GameContent.BlockCrop).GetMethod("OnBlockInteractStart"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_BlockCrop_OnBlockInteractStart_New")));
            harmonyInstance.Patch(typeof(CollectibleObject).GetMethod("OnCreatedByCrafting"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_ItemPlantableSeed_OnCreatedByCrafting")));

            //SEEDS
            /*harmonyInstance.Patch(typeof(Vintagestory.GameContent.ItemPlantableSeed).GetMethod("OnHeldInteractStart"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_ItemPlantableSeed_OnHeldInteractStart")));

            harmonyInstance.Patch(typeof(Vintagestory.GameContent.BlockCrop).GetMethod("OnBlockBroken"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_OnBlockBroken")));

            harmonyInstance.Patch(typeof(Vintagestory.API.Common.Block).GetMethod("OnBlockBroken"), postfix: new HarmonyMethod(typeof(harmPatch).GetMethod("Postfix_Block_OnBlockBroken")));

            Harmony.ReversePatch(typeof(Vintagestory.GameContent.BlockCrop).GetMethod("OnBlockBroken"), new HarmonyMethod(typeof(harmPatch).GetMethod("Stub_Block_OnBlockBroken")));

            harmonyInstance.Patch(typeof(Vintagestory.GameContent.BlockCrop).GetMethod("GetDrops"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_GetDrops")));

            harmonyInstance.Patch(typeof(Vintagestory.GameContent.BlockCrop).GetMethod("OnBlockInteractStart"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_BlockCrop_OnBlockInteractStart")));

            Harmony.ReversePatch(typeof(Block).GetMethod("GetDrops"), new HarmonyMethod(typeof(harmPatch).GetMethod("Stub_GetDrops")));

            harmonyInstance.Patch(typeof(Vintagestory.GameContent.BlockCrop).GetMethod("GetPlacedBlockInfo"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_GetPlacedBlockInfo")));

            harmonyInstance.Patch(typeof(Vintagestory.GameContent.ItemHoe).GetMethod("DoTill"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_DoTill")));

            harmonyInstance.Patch(typeof(BlockWateringCan).GetMethod("OnHeldInteractStep"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_BlockWateringCan_OnHeldInteractStep")));

            harmonyInstance.Patch(typeof(BlockCrop).GetMethod("IsNotOnFarmland", BindingFlags.NonPublic | BindingFlags.Instance), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_BlockCrop_IsNotOnFarmland")));
            */
            loadConfig(api);

            InitColors();
        }
        public static void onCommand(IServerPlayer player, int groupId, CmdArgs args)
        {
            //player.Role.Privileges
            if (!player.Role.Code.Equals("admin"))
            {               
                return;
            }
            if (args.Length < 4)
            {
                return;
            }

            if (!args[0].Equals("stats"))
            {
                return;
            }
            var activeSlot = player.InventoryManager.ActiveHotbarSlot;
            if (activeSlot.Itemstack == null)
            {
                return;
            }

            if(activeSlot.Itemstack.Item is ItemPlantableSeed)
            {
                activeSlot.Itemstack.Attributes.SetInt("g", int.Parse(args[1]));
                activeSlot.Itemstack.Attributes.SetInt("r", int.Parse(args[2]));
                activeSlot.Itemstack.Attributes.SetInt("s", int.Parse(args[3]));
                activeSlot.MarkDirty();
            }
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            sapi = api;

            base.StartServerSide(api);
            
            loadConfig(api);
            api.ChatCommands.Create("cancrops")
                .RequiresPlayer()
                .RequiresPrivilege(Privilege.controlserver)
                .BeginSub("setstat")
                    .WithArgs(api.ChatCommands.Parsers.Word("statName"), api.ChatCommands.Parsers.Int("statVal"))
                    .HandleWith(SetStatsCommands.SetSeedStatCommand)
                 .EndSub();

            InitSharedRegistries(api);

            harmonyInstance = new Harmony(harmonyID);
            harmonyInstance.Patch(typeof(Vintagestory.GameContent.BlockEntityFarmland).GetMethod("updateCropDamage", BindingFlags.NonPublic | BindingFlags.Instance), transpiler: new HarmonyMethod(typeof(harmPatch).GetMethod("Transpiler_BlockEntityFarmland_Update_Cold")));
            harmonyInstance.Patch(typeof(Vintagestory.GameContent.BlockEntityFarmland).GetMethod("updateCropDamage", BindingFlags.NonPublic | BindingFlags.Instance), transpiler: new HarmonyMethod(typeof(harmPatch).GetMethod("Transpiler_BlockEntityFarmland_Update_Heat")));
           // harmonyInstance.Patch(typeof(Vintagestory.GameContent.BlockEntityFarmland).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance), transpiler: new HarmonyMethod(typeof(harmPatch).GetMethod("Transpiler_BlockEntityFarmland_Update")));
            harmonyInstance.Patch(typeof(Vintagestory.GameContent.BlockEntityFastForwardGrowth).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance), transpiler: new HarmonyMethod(typeof(harmPatch).GetMethod("Transpiler_BlockEntityFastForwardGrowth_Update_Light")));
            serverChannel = sapi.Network.RegisterChannel("cancrops");
            serverChannel.RegisterMessageType(typeof(ConfigUpdateValuesPacket));
            api.Event.PlayerJoin += SendUpdatedConfigValues;
        }
        public static void SendUpdatedConfigValues(IServerPlayer player)
        {
            cancrops.serverChannel.SendPacket(
                   new ConfigUpdateValuesPacket()
                   {
                       coldResistanceByStat = cancrops.config.coldResistanceByStat,
                       heatResistanceByStat = cancrops.config.heatResistanceByStat,
                       hiddenGain = cancrops.config.hiddenGain,
                       hiddenGrowth = cancrops.config.hiddenGrowth,
                       hiddenStrength = cancrops.config.hiddenStrength,
                       hiddenResistance = cancrops.config.hiddenResistance,
                       hiddenFertility = cancrops.config.hiddenFertility
                   }
                   , player);
        }
        private void InitSharedRegistries(ICoreAPI api)
        {
            if (agriPlants != null) return; // already initialised for this side

            agriPlants = new AgriPlants();
            agriMutations = new AgriMutations();
            agriMutationHandler = new AgriMutationHandler();

            PopulateRegistries(api);
            agriMutations.BuildComplexityMap();
            RegisterGenes();
            RegisterMutationConditions();
        }

        public void PopulateRegistries(ICoreAPI api)
        {
            InitPlants(api);
            InitMutations(api);
        }


        public void InitPlants(ICoreAPI api)
        {
            api.Logger.VerboseDebug("[cancrops] InitPlants");
            Dictionary<AssetLocation, JToken> many = api.Assets.GetMany<JToken>(api.Logger, "config/plants_jsons");
            
            foreach (KeyValuePair<AssetLocation, JToken> val in many)
            {
                if (val.Value is JObject)
                {
                    JsonAgriPlant readPlant = val.Value.ToObject<JsonAgriPlant>();                   
                    if (readPlant.Enabled)
                    {
                        AgriPlant aPlant = new AgriPlant(readPlant);
                        if (agriPlants.addPlant(aPlant))
                        {
                            api.Logger.VerboseDebug(string.Format("[cancrops] InitPlants::added {0}:{1}", aPlant.Domain, aPlant.Id));
                        }
                    }
                }
              
                if (val.Value is JArray)
                {
                    foreach (JToken token in (val.Value as JArray))
                    {
                        JsonAgriPlant readPlant = token.ToObject<JsonAgriPlant>();
                        if (readPlant.Enabled)
                        {
                            AgriPlant aPlant = new AgriPlant(readPlant);
                            if (agriPlants.addPlant(aPlant))
                            {
                                api.Logger.VerboseDebug(string.Format("[cancrops] InitPlants::added {0}:{1}", aPlant.Domain, aPlant.Id));
                            }
                        }
                    }
                }
            }
        }
        public void InitMutations(ICoreAPI api)
        {
            api.Logger.VerboseDebug("[cancrops] InitMutations");
            Dictionary<AssetLocation, JToken> many = api.Assets.GetMany<JToken>(api.Logger, "config/mutations_jsons");

            foreach (KeyValuePair<AssetLocation, JToken> val in many)
            {
                if (val.Value is JObject)
                {
                    JsonAgriMutation mutation = val.Value.ToObject<JsonAgriMutation>();
                    if (mutation.Enabled)
                    {
                        AgriMutation aMutation = new AgriMutation(mutation);
                        if (agriMutations.AddMutation(aMutation))
                        {
                            api.Logger.VerboseDebug(string.Format("[cancrops] InitMutations::added {0}", aMutation.Child));
                        }
                    }
                }
                if (val.Value is JArray)
                {
                    foreach (JToken token in (val.Value as JArray))
                    {
                        JsonAgriMutation mutation = token.ToObject<JsonAgriMutation>();
                        if (mutation.Enabled)
                        {
                            AgriMutation aMutation = new AgriMutation(mutation);
                            if (agriMutations.AddMutation(aMutation))
                            {
                                api.Logger.VerboseDebug(string.Format("[cancrops] InitMutations::added {0}", aMutation.Child));
                            }
                        }
                    }
                }
            }
        }
        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            //loadConfig(api);
            InitSharedRegistries(api);

            harmonyInstance = new Harmony(harmonyID);

            //SEEDS
            harmonyInstance.Patch(typeof(Vintagestory.GameContent.ItemPlantableSeed).GetMethod("GetHeldItemInfo"), postfix: new HarmonyMethod(typeof(harmPatch).GetMethod("Postfix_ItemPlantableSeed_GetHeldItemInfo")));
            clientChannel = api.Network.RegisterChannel("cancrops");
            clientChannel.RegisterMessageType(typeof(ConfigUpdateValuesPacket));
            clientChannel.SetMessageHandler<ConfigUpdateValuesPacket>((packet) =>
            {
                cancrops.config.coldResistanceByStat = packet.coldResistanceByStat;
                cancrops.config.heatResistanceByStat = packet.heatResistanceByStat;
                cancrops.config.hiddenGain = packet.hiddenGain;
                cancrops.config.hiddenGrowth = packet.hiddenGrowth;
                cancrops.config.hiddenStrength = packet.hiddenStrength;
                cancrops.config.hiddenResistance = packet.hiddenResistance;
                cancrops.config.hiddenFertility = packet.hiddenFertility;
                cancrops.config.hidden_genes["gain"] = cancrops.config.hiddenGain;
                cancrops.config.hidden_genes["growth"] = cancrops.config.hiddenGrowth;
                cancrops.config.hidden_genes["strength"] = cancrops.config.hiddenStrength;
                cancrops.config.hidden_genes["resistance"] = cancrops.config.hiddenResistance;
                cancrops.config.hidden_genes["fertility"] = cancrops.config.hiddenFertility;
                // Rebuild gene registry so IntStatGene.IsHidden reflects the freshly-received
                // server config; tooltip iterates Genome.genes which caches IsHidden per gene.
                RegisterGenes();
                Genome.InvalidateGenesCache();
            });
        }
        public void InitColors()
        {
            int tmpInt;
            foreach(var it in config.gene_color)
            {
                if (CommonUtils.tryFindColor(it.Value, out tmpInt))
                {
                    config.gene_color_int[it.Key] = ColorUtil.Int2Hex(tmpInt);
                }
            }
        }
        private void RegisterGenes()
        {
            GeneRegistry.Clear();
            var intMutator = IntStatMutatorFactory.Create(config.activeStatMutator);
            GeneRegistry.Register(new IntStatGene("gain", config.minGain, config.maxGain, config.hiddenGain, intMutator));
            GeneRegistry.Register(new IntStatGene("growth", config.minGrowth, config.maxGrowth, config.hiddenGrowth, intMutator));
            GeneRegistry.Register(new IntStatGene("strength", config.minStrength, config.maxStrength, config.hiddenStrength, intMutator));
            GeneRegistry.Register(new IntStatGene("resistance", config.minResistance, config.maxResistance, config.hiddenResistance, intMutator));
            GeneRegistry.Register(new IntStatGene("fertility", config.minFertility, config.maxFertility, config.hiddenFertility, intMutator));
            GeneRegistry.Register(new SpeciesGene(agriPlants, agriPlants.getAll(), new SpeciesMutator()));
        }
        private void RegisterMutationConditions()
        {
            ConditionRegistry.Clear();
            ConditionRegistry.Register(LightCondition.TYPE_ID, LightCondition.FromJson);
            ConditionRegistry.Register(ClimateRangeCondition.TYPE_ID, ClimateRangeCondition.FromJson);
            ConditionRegistry.Register(NeighbourBlockCondition.TYPE_ID, NeighbourBlockCondition.FromJson);
            ConditionRegistry.Register(BiomeCondition.TYPE_ID, BiomeCondition.FromJson);
            BiomeProviderRegistry.Set(new LandformBiomeProvider());
        }
        public static AgriPlants GetPlants()
        {
            return agriPlants;
        }
        public static AgriMutations GetMutations()
        {
            return agriMutations;
        }
        public static AgriMutationHandler GetAgriMutationHandler()
        {
            return agriMutationHandler;
        }
        private void loadConfig(ICoreAPI api)
        {
            try
            {
                cancrops.config = api.LoadModConfig<Config>(this.Mod.Info.ModID + ".json");
                api.Logger.VerboseDebug("[cancrops] " + this.Mod.Info.ModID + ".json" + " config loaded.");
                if (cancrops.config != null)
                {
                    api.StoreModConfig<Config>(cancrops.config, this.Mod.Info.ModID + ".json");
                    return;
                }
            }
            catch (Exception e)
            {
                api.Logger.VerboseDebug("[cancrops] " + this.Mod.Info.ModID + ".json" + " config not found." + e);
            }

            cancrops.config = new Config();
            api.StoreModConfig<Config>(cancrops.config, this.Mod.Info.ModID + ".json");
            api.Logger.VerboseDebug("[cancrops] " + this.Mod.Info.ModID + ".json" + " config created and stored.");
            return;
        }
        public override void Dispose()
        {
            base.Dispose();
            harmonyInstance.UnpatchAll(harmonyID);
            // Force re-init on next world load: AgriPlant/AgriMutation hold Block and Item
            // references resolved at InitPlants time. Between worlds those references can
            // point to stale Block instances (different mod sets, different ids), so we
            // drop them here and let InitSharedRegistries rebuild from scratch.
            agriPlants = null;
            agriMutations = null;
            agriMutationHandler = null;
        }
    }
}
