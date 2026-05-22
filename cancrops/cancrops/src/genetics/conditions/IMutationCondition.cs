using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace cancrops.src.genetics.conditions
{
    // A single condition attached to a mutation. Returns FORBID/PASS/FORCE for a given world position.
    // Multiple conditions on a mutation are combined with these rules:
    //   - any FORBID  → mutation forbidden
    //   - all PASS    → roll the mutation chance
    //   - any FORCE   → mutation forced (chance ignored), unless any FORBID also present
    public interface IMutationCondition
    {
        EnumConditionResult Check(IWorldAccessor world, BlockPos pos);
    }
}
