using System.Collections.Generic;
using Vintagestory.API.MathTools;

namespace cancrops.src.implementations
{
    public class AgriBlockCondition
    {
        public AgriBlockCondition(HashSet<int> blockIds, int amount, BlockPos minPos, BlockPos maxPos)
        {
            this.BlockIds = blockIds;
            this.Amount = amount;
            this.MinPos = minPos;
            this.MaxPos = maxPos;
        }
        // Stores all blocks that satisfy the requirement — single block for exact codes,
        // many blocks when the JSON BlockName contains a wildcard (e.g. "game:ore-iron-*").
        public HashSet<int> BlockIds { get; set; }
        public int Amount { get; set; }
        public BlockPos MinPos { get; set; }
        public BlockPos MaxPos { get; set; }
    }
}
