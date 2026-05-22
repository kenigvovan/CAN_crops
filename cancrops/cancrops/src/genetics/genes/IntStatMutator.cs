using System;
using cancrops.src.genetics.abstractions;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace cancrops.src.genetics.genes
{
    public class IntStatMutator : IMutator<int>
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
            int delta = random.Next(2) == 0 ? -step : step;
            return value + delta;
        }
    }
}
