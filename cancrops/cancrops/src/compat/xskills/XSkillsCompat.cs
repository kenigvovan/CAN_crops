using HarmonyLib;
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
using XSkills;

namespace cancrops.src.compat.xskills
{
    public class XSkillsCompat : ModSystem
    {
        public static Harmony harmonyInstance;
        public const string harmonyID = "cancropsxskillscompat.Patches";
        public override double ExecuteOrder()
        {
            return 0.5;
        }
        public override void Start(ICoreAPI api)
        {
            harmonyInstance = new Harmony(harmonyID);

            harmonyInstance.Patch(typeof(XSkillsItemPlantableSeed).GetMethod("OnHeldInteractStart"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_XSkillsItemPlantableSeed_OnHeldInteractStart")));
            Harmony.ReversePatch(typeof(ItemPlantableSeed).GetMethod("OnHeldInteractStart"), new HarmonyMethod(typeof(harmPatch).GetMethod("Stub_ItemPlantableSeed_OnHeldInteractStart")));
            /* var f = harmonyInstance.GetPatchedMethods();
             var c = 3;
             f = Harmony.GetAllPatchedMethods();
             c = 3;
             var or = typeof(BlockWateringCan).GetMethod("OnHeldInteractStep");
             var patches = Harmony.GetPatchInfo(or);
             c = 3;*/
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            harmonyInstance = new Harmony(harmonyID);

            harmonyInstance.Patch(typeof(XSkillsItemHoe).GetMethod("DoTill"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_XSkillsItemHoe_DoTill")));
            Harmony.ReversePatch(typeof(ItemHoe).GetMethod("DoTill"), new HarmonyMethod(typeof(harmPatch).GetMethod("Stub_XSkillsItemHoe_DoTill")));
            var or = typeof(BlockWateringCan).GetMethod("OnHeldInteractStep");
            //patches = Harmony.GetPatchInfo(or);
            harmonyInstance.Unpatch(or, HarmonyPatchType.Postfix);
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            harmonyInstance = new Harmony(harmonyID);

            harmonyInstance.Patch(typeof(XSkills.XSkills).GetMethod("DoHarmonyPatch", BindingFlags.NonPublic | BindingFlags.Static), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Postfix_DoHarmonyPatch")));
        }
    }
}
