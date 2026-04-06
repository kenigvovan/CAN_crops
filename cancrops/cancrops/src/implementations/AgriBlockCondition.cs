using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace cancrops.src.implementations
{
    public class AgriBlockCondition
    {
        public AgriBlockCondition(Block necessaryBlock, int amount, BlockPos minPos, BlockPos maxPos) 
        {
            this.NecessaryBlock = necessaryBlock;
            this.Amount = amount;
            this.MinPos = minPos;
            this.MaxPos = maxPos;
        } 
        public Block NecessaryBlock { get; set; }
        public int Amount { get; set; }
        public BlockPos MinPos { get; set; }
        public BlockPos MaxPos { get; set; }
    }
}
