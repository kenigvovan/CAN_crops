using System;
using cancrops.src.genetics.abstractions;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace cancrops.src.genetics.genes
{
    // Normally-distributed drift around the current value. Always shifts (no chance gate),
    // but most shifts are 0 due to the rounding. Width controlled by config.statMutationSigma.
    public class GaussianIntStatMutator : IMutator<int>
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
            int a = first.Trait + (int)Math.Round(NextGaussian(random) * cancrops.config.statMutationSigma);
            int b = second.Trait + (int)Math.Round(NextGaussian(random) * cancrops.config.statMutationSigma);
            return gene.GeneratePair(gene.GetAllele(a), gene.GetAllele(b));
        }

        // Box-Muller transform; one call returns one standard-normal sample.
        private static double NextGaussian(Random random)
        {
            double u1 = 1.0 - random.NextDouble();
            double u2 = 1.0 - random.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        }
    }
}
