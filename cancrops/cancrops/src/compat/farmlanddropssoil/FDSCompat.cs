using FarmlandDropsSoil;
using HarmonyLib;
using PrimitiveSurvival.ModSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace cancrops.src.compat.farmlanddropssoil
{
    public class FDSCompat: ModSystem
    {
        public static Harmony harmonyInstance;
        public const string harmonyID = "cancropsfarmlanddropssoilcompat.Patches";
        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            harmonyInstance = new Harmony(harmonyID);
            harmonyInstance.Patch(typeof(FarmlandDropsSoilBehavior).GetMethod("GetDrops"), postfix: new HarmonyMethod(typeof(harmPatch).GetMethod("Postfix_FarmlandDropsSoilBehavior_GetDrops")));
            api.Event.ServerRunPhase(EnumServerRunPhase.RunGame, () => AddBehaviorAndSocketNumber(api));
        }
        public void AddBehaviorAndSocketNumber(ICoreServerAPI api)
        {
            var canfarmlandblocks = api.World.SearchBlocks(new AssetLocation("cancrops:farmland-*"));
            foreach(var it in canfarmlandblocks)
            {
                if(!it.HasBehavior<FarmlandDropsSoil.FarmlandDropsSoilBehavior>())
                {
                    it.BlockBehaviors = it.BlockBehaviors.Append(new FarmlandDropsSoilBehavior(it)).ToArray();
                }
            }
        }
    }
}
