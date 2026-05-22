using System.Collections.Generic;
using cancrops.src.BE;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace cancrops.src.utility
{
    public class AgriPlantRequirementChecker
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
            int strength = beCrop.Genome?.Strength?.Dominant?.Value ?? 0;

            int lower = requirements.MinLight - (int)(requirements.LightToleranceFactor * strength);
            int upper = requirements.MaxLight + (int)(requirements.LightToleranceFactor * strength);

            if(lightLevel < lower || lightLevel > upper)
            {
                return false;
            }

            // Empty/missing block conditions = no spatial requirement → pass.
            if (requirements.Conditions == null || requirements.Conditions.Count == 0)
            {
                return true;
            }

            // Stage gating: until the plant reaches RequirementFromStage the block-conditions
            // are inactive (the plant sprouts wherever, then needs its terrain block to mature).
            if (requirements.RequirementFromStage > 0)
            {
                int currentStage = beCrop.GetCropStageWithout();
                if (currentStage < requirements.RequirementFromStage) return true;
            }

            // Each condition supplies a set of acceptable block ids (single block for exact
            // codes, many for wildcards). Any block in that set counts toward Amount —
            // e.g. a wildcard "game:ore-iron-*" treats all rock variants as the same ore.
            bool conditionSatisfied = false;
            foreach (var condition in requirements.Conditions)
            {
                int found = 0;
                cancrops.sapi.World.BlockAccessor.SearchBlocks(
                    beCrop.Pos.AddCopy(condition.MinPos.X, condition.MinPos.Y, condition.MinPos.Z),
                    beCrop.Pos.AddCopy(condition.MaxPos.X, condition.MaxPos.Y, condition.MaxPos.Z),
                    delegate (Block block, BlockPos pos)
                {
                    if (condition.BlockIds != null && condition.BlockIds.Contains(block.Id))
                    {
                        found++;
                        if (found >= condition.Amount)
                        {
                            conditionSatisfied = true;
                            return false;
                        }
                    }
                    return true;
                });
                if (conditionSatisfied) break;
            }
            if (!conditionSatisfied)
            {
                return false;
            }


            return true;
        }
    }
}
