using System;
using System.Collections.Generic;
using System.Linq;
using cancrops.src.BE;

namespace cancrops.src.genetics
{
    public class ParentSelector
    {
        public List<CANBECrop> selectAndOrder(IEnumerable<CANBECrop> neighbours, Random random)
        {
            return neighbours
                .Where(x => x?.agriPlant != null)
                .Where(x => x.GetCropStageWithout() >= x.agriPlant.AllowSourceStage)
                // Highest fertility first
                .OrderByDescending(GetFertility)
                .Where(x => RollFertility(x, random))
                .ToList();
        }

        private static int GetFertility(CANBECrop crop)
        {
            return crop?.Genome?.Fertility?.Dominant?.Value ?? 0;
        }

        private static bool RollFertility(CANBECrop crop, Random random)
        {
            int max = cancrops.config.maxFertility;
            if (max <= 0) return true;
            int fert = GetFertility(crop);
            // Floor: even at 0 fertility, allow a small chance so wild plants are not sterile.
            if (fert == 0) return random.NextDouble() < 0.05;
            return random.Next(max) < fert;
        }
    }
}
