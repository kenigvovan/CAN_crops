using Newtonsoft.Json.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace cancrops.src.genetics.conditions.types
{
    // Counts blocks of a given AssetLocation within a relative box around the cross-sticks position.
    // Met if `amount` or more matching blocks are found.
    public class NeighbourBlockCondition : IMutationCondition
    {
        public const string TYPE_ID = "neighbour_block";

        public AssetLocation BlockCode { get; }
        public int Amount { get; }
        public BlockPos MinOffset { get; }
        public BlockPos MaxOffset { get; }
        public EnumConditionResult MetResult { get; }

        public NeighbourBlockCondition(AssetLocation blockCode, int amount, BlockPos minOffset, BlockPos maxOffset, EnumConditionResult metResult)
        {
            BlockCode = blockCode;
            Amount = amount;
            MinOffset = minOffset;
            MaxOffset = maxOffset;
            MetResult = metResult;
        }

        public EnumConditionResult Check(IWorldAccessor world, BlockPos pos)
        {
            int found = 0;
            BlockPos min = pos.AddCopy(MinOffset.X, MinOffset.Y, MinOffset.Z);
            BlockPos max = pos.AddCopy(MaxOffset.X, MaxOffset.Y, MaxOffset.Z);
            world.BlockAccessor.SearchBlocks(min, max, (block, p) =>
            {
                if (block?.Code != null && block.Code.Equals(BlockCode))
                {
                    found++;
                    if (found >= Amount) return false;
                }
                return true;
            });
            bool met = found >= Amount;
            return met ? MetResult : EnumConditionResult.FORBID;
        }

        public static IMutationCondition FromJson(JObject json)
        {
            string code = (string)json["block"];
            int amount = (int?)json["amount"] ?? 1;
            var min = new BlockPos((int?)json["minX"] ?? -1, (int?)json["minY"] ?? -1, (int?)json["minZ"] ?? -1, 0);
            var max = new BlockPos((int?)json["maxX"] ?? 1,  (int?)json["maxY"] ?? 1,  (int?)json["maxZ"] ?? 1,  0);
            var result = ConditionJsonHelper.ParseResult(json["result"], EnumConditionResult.PASS);
            return new NeighbourBlockCondition(new AssetLocation(code), amount, min, max, result);
        }
    }
}
