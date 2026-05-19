using System;
using cancrops.src.genetics.abstractions;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace cancrops.src.genetics.genes
{
    // Returns the (first, second) pair untouched — no drift across generations.
    // Useful for servers with frozen genetics or for testing.
    public class NoneIntStatMutator : IMutator<int>
    {
        public IGenePair<int> PickOrMutate(
            IGene<int> gene,
            IAllele<int> first,
            IAllele<int> second,
            (IGenome a, IGenome b) parents,
            BlockPos pos,
            IWorldAccessor world,
            Random random)
        {
            return gene.GeneratePair(first, second);
        }
    }
}
