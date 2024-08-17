using HarmonyLib;
using PrimitiveSurvival.ModSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace cancrops.src.compat.primitivesurvival
{
    public class PSCompat: ModSystem
    {
        public static Harmony harmonyInstance;
        public const string harmonyID = "cancropsprimitivesurvivalcompat.Patches";
        public override void Start(ICoreAPI api)
        {
            harmonyInstance = new Harmony(harmonyID);
            //itemhoeextended
            harmonyInstance.Patch(typeof(ItemHoeExtended).GetMethod("DoTill"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_ItemHoeExtended_DoTill")));
            harmonyInstance.Patch(typeof(ItemHoeExtended).GetMethod("OnHeldInteractStart"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_ItemHoeExtended_OnHeldInteractStart")));
            Harmony.ReversePatch(typeof(ItemHoe).GetMethod("OnHeldInteractStart"), new HarmonyMethod(typeof(harmPatch).GetMethod("Stub_OnHeldInteractStart")));
            //befurrowedland
            harmonyInstance.Patch(typeof(BEFurrowedLand).GetMethod("OnPipeTick", BindingFlags.NonPublic | BindingFlags.Instance), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_OnPipeTick")));
            //BEIRRIGATIONVESSEL
            harmonyInstance.Patch(typeof(BEIrrigationVessel).GetMethod("IrrigationVesselUpdate"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_IrrigationVesselUpdate")));
            //besupport
            harmonyInstance.Patch(typeof(BESupport).GetMethod("TryWaterFarmland", BindingFlags.NonPublic | BindingFlags.Instance), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_TryWaterFarmland")));
            //BLOCKsupport
            harmonyInstance.Patch(typeof(BlockSupport).GetMethod("TryPlaceBlock"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_TryPlaceBlock")));
            //BLOCKsupport
            //EntityAgent
            //BlockWateringCan
            Harmony.ReversePatch(typeof(EntityAgent).GetMethod("OnGameTick"), new HarmonyMethod(typeof(harmPatch).GetMethod("Stub_OnGameTick")));
            harmonyInstance.Patch(typeof(EntityEarthworm).GetMethod("OnGameTick"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_OnGameTick")));
            Harmony.ReversePatch(typeof(CollectibleObject).GetMethod("OnHeldInteractStart"), new HarmonyMethod(typeof(harmPatch).GetMethod("Stub_Item_OnHeldInteractStart")));
            harmonyInstance.Patch(typeof(ItemEarthworm).GetMethod("OnHeldInteractStart"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_ItemEarthworm_OnHeldInteractStart")));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {

        }

        public override void StartClientSide(ICoreClientAPI api)
        {

        }
    }
}
