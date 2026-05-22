using System;
using cancrops.src.genetics.abstractions;
using cancrops.src.genetics.genes;
using cancrops.src.implementations;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace cancrops.src.genetics
{
    // AgriCraft-style single-loop cross-breed engine. Iterates every gene in GeneRegistry and
    // delegates to gene-specific dispatch (IGene.CrossBreed) to mutate per the gene's mutator.
    public class CrossBreedEngine
    {
        public IGenome Combine(IGenome a, IGenome b, BlockPos pos, IWorldAccessor world, Random random)
        {
            var result = new MapGenome();
            foreach (var gene in GeneRegistry.AllGenes)
            {
                IGenePair pairA = a.GetGenePair(gene);
                IGenePair pairB = b.GetGenePair(gene);

                if (pairA == null && pairB == null) continue;
                if (pairA == null) { result.SetGenePair(pairB.Clone()); continue; }
                if (pairB == null) { result.SetGenePair(pairA.Clone()); continue; }

                var newPair = gene.CrossBreed(pairA, pairB, a, b, pos, world, random);
                if (newPair != null) result.SetGenePair(newPair);
            }
            return result;
        }

        public IGenome Clone(IGenome parent, BlockPos pos, IWorldAccessor world, Random random)
        {
            if (!cancrops.config.cloneMutations)
            {
                return parent.Clone();
            }
            var result = new MapGenome();
            foreach (var gene in GeneRegistry.AllGenes)
            {
                IGenePair pair = parent.GetGenePair(gene);
                if (pair == null) continue;
                var newPair = gene.CrossBreed(pair, pair, parent, parent, pos, world, random);
                if (newPair != null) result.SetGenePair(newPair);
            }
            return result;
        }

        // Convenience helper: pulls AgriPlant out of the species gene of a genome, or null.
        public static AgriPlant GetSpecies(IGenome genome)
        {
            var gene = GeneRegistry.Get<AgriPlant>(SpeciesGene.GENE_ID);
            if (gene == null) return null;
            return genome.GetGenePair<AgriPlant>(gene)?.GetTrait();
        }
    }
}
