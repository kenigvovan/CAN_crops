using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace cancrops.src.genetics.abstractions
{
    public interface IMutator<T>
    {
        IGenePair<T> PickOrMutate(
            IGene<T> gene,
            IAllele<T> first,
            IAllele<T> second,
            (IGenome a, IGenome b) parents,
            BlockPos pos,
            IWorldAccessor world,
            Random random);
    }
}
