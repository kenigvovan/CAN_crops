using System;
using cancrops.src.genetics.abstractions;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace cancrops.src.genetics.genes
{
    // Acts like the default mutator, but if the input value is 0 there's a configurable chance
    // (config.statResetOnZeroChance) to bump it up to a random allele in [1, max]. Lets a
    // genetically dead lineage occasionally recover.
    public class ResetOnLowIntStatMutator : IMutator<int>
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
            int max = (gene is IntStatGene intGene) ? intGene.Max : 10;
            int a = MaybeMutate(first.Trait, max, random);
            int b = MaybeMutate(second.Trait, max, random);
            return gene.GeneratePair(gene.GetAllele(a), gene.GetAllele(b));
        }

        private int MaybeMutate(int value, int max, Random random)
        {
            if (value == 0 && random.NextDouble() < cancrops.config.statResetOnZeroChance)
            {
                return 1 + random.Next(max);
            }
            if (random.NextDouble() >= cancrops.config.statMutationChance) return value;
            int step = cancrops.config.statMutationStep;
            int delta = random.Next(2) == 0 ? -step : step;
            return value + delta;
        }
    }
}
