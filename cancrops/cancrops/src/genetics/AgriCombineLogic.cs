using cancrops.src.BE;
using System;
using System.Collections.Generic;

namespace cancrops.src.genetics
{
    public class AgriCombineLogic
    {
        public Genome combine(CANBECrossSticks crop, Tuple<Genome, Genome> parents, Random random)
        {
            List<Gene> geneList = new List<Gene>();
            foreach (var gene in parents.Item1)
            {
                Gene geneA = parents.Item1.GetGeneByName(gene.StatName);
                Gene geneB = parents.Item2.GetGeneByName(gene.StatName);
                int mutativityA = parents.Item1.Mutativity.Dominant.Value;
                int mutativityB = parents.Item2.Mutativity.Dominant.Value;

                // Each parent passes one random allele (50/50 Dominant or Recessive)
                int alleleFromA = random.Next(2) == 0 ? geneA.Dominant.Value : geneA.Recessive.Value;
                int alleleFromB = random.Next(2) == 0 ? geneB.Dominant.Value : geneB.Recessive.Value;

                // Apply mutations
                alleleFromA = MutationUtils.MutateAllele(alleleFromA, mutativityA, random);
                alleleFromB = MutationUtils.MutateAllele(alleleFromB, mutativityB, random);

                // Higher value becomes Dominant
                int d = Math.Max(alleleFromA, alleleFromB);
                int r = Math.Min(alleleFromA, alleleFromB);
                geneList.Add(new Gene(gene.StatName, new Allele(d), new Allele(r)));
            }
            return new Genome(geneList);
        }
    }
}
