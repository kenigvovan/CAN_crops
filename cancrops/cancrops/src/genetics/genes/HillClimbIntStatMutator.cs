using System;
using cancrops.src.genetics.abstractions;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace cancrops.src.genetics.genes
{
    // Like default, but mutations are biased upward by config.statMutationUpBias.
    // Restores the "stats drift up over generations" feel without a Mutativity gene.
    public class HillClimbIntStatMutator : IMutator<int>
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
            int a = MaybeMutate(first.Trait, random);
            int b = MaybeMutate(second.Trait, random);
            return gene.GeneratePair(gene.GetAllele(a), gene.GetAllele(b));
        }

        private int MaybeMutate(int value, Random random)
        {
            if (random.NextDouble() >= cancrops.config.statMutationChance) return value;
            int step = cancrops.config.statMutationStep;
            double upBias = cancrops.config.statMutationUpBias;
            int delta = random.NextDouble() < upBias ? step : -step;
            return value + delta;
        }
    }
}
