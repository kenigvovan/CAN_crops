using System.Collections.Generic;
using cancrops.src.BE;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace cancrops.src.utility
{
    public class AgriPlantRequirmentChecker
    {
        public static bool CheckAgriPlantRequirements(CANBECrop beCrop)
        {
            if(beCrop.agriPlant == null)
            {
                return false;
            }
            if(beCrop.agriPlant.Requirement == null)
            {
                return true;
            }
            var agriPlant = beCrop.agriPlant;
            var requirements = agriPlant.Requirement;
            int lightLevel = cancrops.sapi.World.BlockAccessor.GetLightLevel(beCrop.Pos, (EnumLightLevelType)requirements.LightLevelType);
            var plantStats = beCrop.Genome;

            int lower = requirements.MinLight - (int)(requirements.LightToleranceFactor * plantStats.Strength.Dominant.Value);
            int upper = requirements.MaxLight + (int)(requirements.LightToleranceFactor * plantStats.Strength.Dominant.Value);

            if(lightLevel < lower || lightLevel > upper)
            {
                return false;
            }
            //find how to check for block of kind in area
            bool conditionSatisfied = false;
            Dictionary<int, int> neccessaryBlocksCounters = new Dictionary<int, int>();
            foreach (var condition in requirements.Conditions) 
            {
                cancrops.sapi.World.BlockAccessor.SearchBlocks(beCrop.Pos.AddCopy(condition.MinPos.X, condition.MinPos.Y + 1, condition.MinPos.Z), beCrop.Pos.AddCopy(condition.MaxPos.X, condition.MaxPos.Y, condition.MaxPos.Z), delegate (Block block, BlockPos pos)
                {
                    if (block.Id == condition.NecessaryBlock.Id)
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
                        else
                        {
                            conditionSatisfied = true;
                            return false;
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
