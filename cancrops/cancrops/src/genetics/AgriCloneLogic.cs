using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cancrops.src.genetics
{
    public class AgriCloneLogic
    {
        public Genome clone(Genome parent, Random random)
        {
            return new Genome(parent.AsEnumerable().ToList().ConvertAll(gene => this.cloneGene(gene, parent, random)));
        }
        protected Gene cloneGene(Gene gene, Genome parent, Random rand)
        {
            if (cancrops.config.cloneMutations)
            {
                return cancrops.GetAgriMutationHandler()
                    .GetStatMutator()
                    .pickOrMutate(
                        gene,
                        new Tuple<Genome, Genome>(parent, parent),
                        rand
                );
            }
            else
            {
                return gene.Clone();
            }
        }
    }
}
