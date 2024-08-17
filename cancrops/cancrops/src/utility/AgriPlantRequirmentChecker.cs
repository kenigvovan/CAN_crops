using cancrops.src.blockenities;
using cancrops.src.templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace cancrops.src.utility
{
    public class AgriPlantRequirmentChecker
    {
        public static bool CheckAgriPlantRequirements(CANBlockEntityFarmland farmland)
        {
            //farmland is null? no agriplant on selected farmland found
            if (farmland == null || !farmland.HasAgriPlant() || farmland.agriPlant.Requirement == null) 
            {
                return false;
            }

            var agriPlant = farmland.agriPlant;
            var requirements = agriPlant.Requirement;
            int lightLevel = cancrops.sapi.World.BlockAccessor.GetLightLevel(farmland.Position.AsBlockPos, (EnumLightLevelType)requirements.LightLevelType);
            var plantStats = farmland.Genome;

            int lower = requirements.MinLight - (int)(requirements.LightToleranceFactor * plantStats.Strength.Dominant.Value);
            int upper = requirements.MaxLight + (int)(requirements.LightToleranceFactor * plantStats.Strength.Dominant.Value);

            if(lightLevel < lower || lightLevel > upper)
            {
                return false;
            }




            return true;
        }
    }
}
