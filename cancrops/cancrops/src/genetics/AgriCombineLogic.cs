using cancrops.src.blockenities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cancrops.src.genetics
{
    public class AgriCombineLogic
    {
        public Genome combine(CANBlockEntityFarmland crop, Tuple<Genome, Genome> parents, Random random)
        {
            List<Gene> geneList = new List<Gene>();
            var firstGenome = parents.Item1.GetEnumerator();
            var secondGenome = parents.Item1.GetEnumerator();
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
        protected Gene mutateGene(Gene gene, Tuple<Genome, Genome> parents, Random rand)
        {
            return
                   new Gene(gene.StatName,
                            this.pickRandomAllele(parents.Item1.GetGeneByName(gene.StatName), parents.Item1.Mutativity.Dominant.Value, rand),
                            this.pickRandomAllele(parents.Item2.GetGeneByName(gene.StatName), parents.Item2.Mutativity.Dominant.Value, rand),
                            gene.Hidden);
        }
        protected Allele pickRandomAllele(Gene pair, int statValue, Random random)
        {
            var allele = (random.Next(1) > 0) ? new Allele(pair.Dominant.Value): new Allele(pair.Recessive.Value);
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
                else if(newValue > 10) //TODO add dict with max values
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
