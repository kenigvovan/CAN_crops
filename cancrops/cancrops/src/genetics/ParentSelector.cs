using cancrops.src.BE;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cancrops.src.genetics
{
    public class ParentSelector
    {
        public List<CANBECrop> selectAndOrder(IEnumerable<CANBECrop> neighbours, Random random)
        {
            return neighbours
                    .Where(x => x.agriPlant != null)
                    // Mature crops only
                    .Where(x => x.GetCropStageWithout() >= x.agriPlant.AllowSourceStage)
                    // Sort based on fertility stat (highest fertility first)
                    .OrderByDescending(sorter)
                    // Roll for fertility stat
                    .Where(x => this.rollFertility(x, random))
                    .ToList();
        }
        protected int sorter(CANBECrop crop)
        {
            return cancrops.config.maxFertility - crop?.Genome.Fertility.Dominant.Value ?? 1;
        }
        protected bool rollFertility(CANBECrop crop, Random random)
        {
            int tm = random.Next(cancrops.config.maxFertility);
            return tm < (crop?.Genome.Fertility.Dominant.Value ?? 1);
        }
    }
}
