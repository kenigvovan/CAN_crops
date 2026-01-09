using cancrops.src.BE;
using System;
using System.Collections.Generic;

namespace cancrops.src.genetics
{
    // Handles the genetic crossbreeding logic for combining two parent genomes.
    // Uses Mendelian genetics principles with mutation possibilities.
    public class AgriCombineLogic
    {
        // Combines two parent genomes to create an offspring genome.
        // For each gene, randomly selects alleles from both parents and applies potential mutations.
        // "crop" The cross sticks block entity where breeding occurs
        // "parents" Tuple containing both parent genomes
        // Returns a new genome representing the offspring
        public Genome combine(CANBECrossSticks crop, Tuple<Genome, Genome> parents, Random random)
        {
            List<Gene> geneList = new List<Gene>();
            var firstGenome = parents.Item1.GetEnumerator();
            var secondGenome = parents.Item2.GetEnumerator();
            firstGenome.MoveNext();
            secondGenome.MoveNext();
            //really why tuple used even deeper
            do
            {
                geneList.Add(mutateGene(firstGenome.Current, parents, random));
            }
            while (firstGenome.MoveNext() && secondGenome.MoveNext());
            
            foreach(var gen1 in parents.Item1)
            {
                foreach(var gen2 in parents.Item2)
                {
                    geneList.Add(mutateGene(gen1, parents, random));
                    
                    break;
                }
            }
            return new Genome(geneList);
        }
        // Creates a new gene by combining genetic material from both parents.
        // Randomly selects alleles from parent genes and applies potential mutations.
        protected Gene mutateGene(Gene gene, Tuple<Genome, Genome> parents, Random rand)
        {
            return
                   new Gene(gene.StatName,
                            this.pickRandomAllele(parents.Item1.GetGeneByName(gene.StatName), parents.Item1.Mutativity.Dominant.Value, rand),
                            this.pickRandomAllele(parents.Item2.GetGeneByName(gene.StatName), parents.Item2.Mutativity.Dominant.Value, rand));
        }
        // Selects a random allele from a gene pair with potential for mutation.
        // Mutation probability is influenced by the mutativity stat:
        // - Higher mutativity = higher chance of positive mutations
        // - Mutativity of 1: 25% positive / 50% no change / 25% negative
        // - Mutativity of 10: 100% positive mutation chance
        // "pair" The gene to select an allele from
        // "statValue" The mutativity stat value (0-10)
        // Returns a potentially mutated allele
        protected Allele pickRandomAllele(Gene pair, int statValue, Random random)
        {
            var allele = (random.Next(6) > 0) ? new Allele(pair.Dominant.Value): new Allele(pair.Recessive.Value);
            
            // Mutation logic: Mutativity stat affects probability and direction of mutations
            // Mutativity stat of 1 results in 25/50/25 probability of positive/no/negative mutation
            // Mutativity stat of 10 results in 100/0/0 probability of positive/no/negative mutation
            int max = cancrops.config.maxMutativity;
            if (random.Next(max) > statValue)
            {
                int delta = random.Next(max) < (max + statValue) / 2 ? 1 : -1;
                int newValue = delta + allele.Value;
                if (newValue <= 0)
                {
                    return new Allele(1);
                }
                else if(newValue > 10) // Cap at maximum value (should use config max)
                {
                    return new Allele(10);
                }
                return new Allele(allele.Value + delta);
            }
            else
            {
                return allele;
            }
        }
	}
}
