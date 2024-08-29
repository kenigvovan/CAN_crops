using cancrops.src.blockenities;
using cancrops.src.templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace cancrops.src.utility
{
    public class AgriPlantRequirmentChecker
    {
        public static bool CheckAgriPlantRequirements(CANBlockEntityFarmland farmland)
        {
            //farmland is null? no agriplant on selected farmland found
            if (farmland == null || !farmland.HasAgriPlant()) 
            {
                return false;
            }
            if(farmland.agriPlant.Requirement == null)
            {
                return true;
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
            //find how to check for block of kind in area
            bool conditionSatisfied = true;
            Dictionary<int, int> neccessaryBlocksCounters = new Dictionary<int, int>();
            foreach (var condition in requirements.Conditions) 
            {
                cancrops.sapi.World.BlockAccessor.SearchBlocks(farmland.Pos.AddCopy(condition.MinPos.X, condition.MinPos.Y, condition.MinPos.Z), farmland.Pos.AddCopy(condition.MaxPos.X, condition.MaxPos.Y, condition.MaxPos.Z), delegate (Block block, BlockPos pos)
                {
                    if (block == condition.NecessaryBlock)
                    {
                        if(condition.Amount > 1)
                        {
                            if(neccessaryBlocksCounters.TryGetValue(condition.NecessaryBlock.Id, out int alreadyFoundAmount))
                            {
                                if(alreadyFoundAmount + 1 >= condition.Amount)
                                {
                                    conditionSatisfied = true;
                                    return false;
                                }
                                else
                                {
                                    neccessaryBlocksCounters[condition.NecessaryBlock.Id] += 1;
                                }
                            }
                            else
                            {
                                neccessaryBlocksCounters[condition.NecessaryBlock.Id] = 1;
                            }
                        }
                    }
                    return true;
                });
                if(conditionSatisfied)
                {
                    break;
                }
            }
            if (!conditionSatisfied)
            {
                return false;
            }


            return true;
        }
    }
}
