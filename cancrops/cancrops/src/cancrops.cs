using cancrops.src.blockenities;
using cancrops.src.blocks;
using cancrops.src.commands;
using cancrops.src.genetics;
using cancrops.src.items;
using cancrops.src.templates;
using cancrops.src.utility;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.CommandAbbr;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using weightmod.src;

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
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            //api.RegisterBlockClass("CANBlockCrop", typeof(CANBlockCrop));

            //Items
            //api.RegisterItemClass("CANItemPlantableSeed", typeof(CANItemPlantableSeed));
            api.RegisterItemClass("CANItemSelectionSticks", typeof(CANItemSelectionSticks));
            //api.RegisterItemClass("CANItemHoe", typeof(CANItemHoe));

            //Blocks
            api.RegisterBlockClass("CANBlockFarmland", typeof(CANBlockFarmland));
            //api.RegisterBlockClass("CANBlockSelectionSticks", typeof(CANBlockSelectionSticks));

            //BE
            api.RegisterBlockEntityClass("CANBlockEntityFarmland", typeof(CANBlockEntityFarmland));

            //Patches
            harmonyInstance = new Harmony(harmonyID);

            //SEEDS
            harmonyInstance.Patch(typeof(Vintagestory.GameContent.ItemPlantableSeed).GetMethod("OnHeldInteractStart"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_ItemPlantableSeed_OnHeldInteractStart")));


           // Harmony.ReversePatch(typeof(Vintagestory.API.Common.Block).GetMethod("OnBlockInteractStart"), new HarmonyMethod(typeof(harmPatch).GetMethod("Stub_Block_OnBlockInteractStart")));
            
           // harmonyInstance.Patch(typeof(Vintagestory.GameContent.BlockFarmland).GetMethod("OnBlockInteractStart"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_BlockFarmland_OnBlockInteractStart")));

            //harmonyInstance.Patch(typeof(Vintagestory.GameContent.BlockCrop).GetMethod("OnBlockInteractStart"), transpiler: new HarmonyMethod(typeof(harmPatch).GetMethod("Transpiler_OnBlockInteractStart")));

           
            
            harmonyInstance.Patch(typeof(Vintagestory.GameContent.BlockCrop).GetMethod("OnBlockBroken"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_OnBlockBroken")));
            
            Harmony.ReversePatch(typeof(Vintagestory.GameContent.BlockCrop).GetMethod("OnBlockBroken"), new HarmonyMethod(typeof(harmPatch).GetMethod("Stub_Block_OnBlockBroken")));

            harmonyInstance.Patch(typeof(Vintagestory.GameContent.BlockCrop).GetMethod("GetDrops"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_GetDrops")));
            
            Harmony.ReversePatch(typeof(Block).GetMethod("GetDrops"), new HarmonyMethod(typeof(harmPatch).GetMethod("Stub_GetDrops")));

            harmonyInstance.Patch(typeof(Vintagestory.GameContent.BlockCrop).GetMethod("GetPlacedBlockInfo"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_GetPlacedBlockInfo")));

            harmonyInstance.Patch(typeof(Vintagestory.GameContent.ItemHoe).GetMethod("DoTill"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_DoTill")));

            harmonyInstance.Patch(typeof(BlockWateringCan).GetMethod("OnHeldInteractStep"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_BlockWateringCan_OnHeldInteractStep")));

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
                    //.RequiresPrivilege(Privilege.chat)
                    .WithArgs(api.ChatCommands.Parsers.Word("statName"), api.ChatCommands.Parsers.Int("statVal"))
                    .HandleWith(SetStatsCommands.SetSeedStatCommand)
                 .EndSub();

            //api.RegisterCommand("cancrops", "", "", onCommand, Privilege.controlserver);
            harmonyInstance = new Harmony(harmonyID);
            harmonyInstance.Patch(typeof(Vintagestory.GameContent.ItemHoe).GetMethod("DoTill"), transpiler: new HarmonyMethod(typeof(harmPatch).GetMethod("Transpiler_ItemHoe_DoTill")));

            harmonyInstance.Patch(typeof(Vintagestory.GameContent.ItemPlantableSeed).GetMethod("OnLoaded"), transpiler: new HarmonyMethod(typeof(harmPatch).GetMethod("Transpiler_OnLoaded")));
          
            agriPlants = new AgriPlants();
            agriMutations = new AgriMutations();
            agriMutationHandler = new AgriMutationHandler();

            PopulateRegistries(api);

        }
        public void PopulateRegistries(ICoreServerAPI api)
        {           
            InitPlants(api);
            InitMutations(api);
        }


        public void InitPlants(ICoreServerAPI api)
        {
            api.Logger.Debug("[cancrops] InitPlants");
            Dictionary<AssetLocation, JToken> many = api.Assets.GetMany<JToken>(api.Server.Logger, "recipes/plants_jsons");
            
            foreach (KeyValuePair<AssetLocation, JToken> val in many)
            {
                if (val.Value is JObject)
                {
                    var readPlant = val.Value.ToObject<AgriPlant>();
                    if(readPlant.Enabled)
                    {
                        //plants.addPlant(readPlant);
                        if (agriPlants.addPlant(readPlant))
                        {
                            api.Logger.Debug(string.Format("[cancrops] InitPlants::added {0}", readPlant.Id));
                        }
                    }
                }
                if (val.Value is JArray)
                {
                    foreach (JToken token in (val.Value as JArray))
                    {
                        var readPlant = token.ToObject<AgriPlant>();
                        if (readPlant.Enabled)
                        {
                            if(agriPlants.addPlant(readPlant))
                            {
                                api.Logger.Debug(string.Format("[cancrops] InitPlants::added {0}", readPlant.Id));
                            }
                        }
                    }
                }
            }
        }
        public void InitMutations(ICoreServerAPI api)
        {
            api.Logger.Debug("[cancrops] InitMutations");
            Dictionary<AssetLocation, JToken> many = api.Assets.GetMany<JToken>(api.Server.Logger, "recipes/mutations_jsons");

            foreach (KeyValuePair<AssetLocation, JToken> val in many)
            {
                if (val.Value is JObject)
                {
                    var mutations = val.Value.ToObject<AgriMutation>();
                    if (mutations.Enabled)
                    {
                        //plants.addPlant(readPlant);
                        if (agriMutations.AddMutation(mutations))
                        {
                            api.Logger.Debug(string.Format("[cancrops] InitMutations::added {0}", mutations.Child));
                        }
                    }
                }
                if (val.Value is JArray)
                {
                    foreach (JToken token in (val.Value as JArray))
                    {
                        var mutations = token.ToObject<AgriMutation>();
                        if (mutations.Enabled)
                        {
                            if (agriMutations.AddMutation(mutations))
                            {
                                api.Logger.Debug(string.Format("[cancrops] InitMutations::added {0}", mutations.Child));
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
            harmonyInstance = new Harmony(harmonyID);

            //SEEDS
            harmonyInstance.Patch(typeof(Vintagestory.GameContent.ItemPlantableSeed).GetMethod("GetHeldItemInfo"), postfix: new HarmonyMethod(typeof(harmPatch).GetMethod("Postfix_ItemPlantableSeed_GetHeldItemInfo")));

            Harmony.ReversePatch(typeof(Vintagestory.API.Common.Item).GetMethod("GetHeldItemInfo"), new HarmonyMethod(typeof(harmPatch).GetMethod("Stub_Item_GetHeldItemInfo")));
            Harmony.ReversePatch(typeof(Vintagestory.API.Common.RegistryObject).GetMethod("CodeWithPath"), new HarmonyMethod(typeof(harmPatch).GetMethod("Stub_RegistryObject_CodeWithPath")));
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
                api.Logger.Debug("[cancrops] " + this.Mod.Info.ModID + ".json" + " config loaded.");
                if (cancrops.config != null)
                {
                    return;
                }
            }
            catch (Exception e)
            {
                api.Logger.Debug("[cancrops] " + this.Mod.Info.ModID + ".json" + " config not found.");
            }

            cancrops.config = new Config();
            api.StoreModConfig<Config>(cancrops.config, this.Mod.Info.ModID + ".json");
            api.Logger.Debug("[cancrops] " + this.Mod.Info.ModID + ".json" + " config created and stored.");
            return;
        }
        public override void Dispose()
        {
            base.Dispose();
            harmonyInstance.UnpatchAll(harmonyID);
        }
    }
}
