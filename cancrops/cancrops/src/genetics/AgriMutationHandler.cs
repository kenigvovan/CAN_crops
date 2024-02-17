using cancrops.src.blockenities;
using cancrops.src.templates;
using cancrops.src.utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cancrops.src.genetics
{
    public class AgriMutationHandler
    {
        private ParentSelector selector;
        private AgriStatMutator statMutator;
        private AgriPlantMutator plantMutator;
        private AgriCloneLogic cloner;
        private AgriCombineLogic combiner;
        protected ParentSelector getSelector()
        {
            return this.selector;
        }
        public AgriStatMutator GetStatMutator()
        {
            return statMutator;
        }
        public AgriPlantMutator GetPlantMutator()
        {
            return plantMutator;
        }
        public AgriMutationHandler()
        {
            selector = new ParentSelector();
            statMutator = new AgriStatMutator();
            plantMutator = new AgriPlantMutator();
            cloner = new AgriCloneLogic();
            combiner = new AgriCombineLogic();
        }

        public bool handleCrossBreedTick(CANBlockEntityFarmland crop, IEnumerable<CANBlockEntityFarmland> neighbours, Random random)
        {
            // select candidate parents from the neighbours
            List<CANBlockEntityFarmland> candidates = this.getSelector().selectAndOrder(neighbours, random);
            // No candidates: do nothing
            if (candidates.Count() <= 0)
            {
                return false;
            }
            // Only one candidate: clone
            if (candidates.Count() == 1)
            {
                return this.doClone(crop, candidates[0], random);
            }
            // More than one candidate passed, pick the two parents with the highest fertility stat:
            return this.doCombine(crop, candidates[0], candidates[1], random);
        }

        protected bool doClone(CANBlockEntityFarmland target, CANBlockEntityFarmland parent, Random random)
        {
            AgriPlant plant = parent.agriPlant;
            // Try spawning a clone if cloning is allowed
            if (plant.AllowCloning)
            {
                // roll for spread chance
                if (random.NextDouble() < parent.agriPlant.SpreadChance)
                {
                    return this.spawnChild(target, this.cloner.clone(parent.Genome, random), plant);
                }
            }
            // spreading failed
            return false;
        }
        protected bool spawnChild(CANBlockEntityFarmland target, Genome genome, AgriPlant agriPlant)
        {
            return target.spawnGenome(genome, agriPlant);
        }
        protected bool doCombine(CANBlockEntityFarmland target, CANBlockEntityFarmland a, CANBlockEntityFarmland b, Random random)
        {
            Genome aGenome = a.Genome;
            Genome bGenome = b.Genome;

            // Determine the child's genome
            Genome resultGenome = this.combiner.combine(target, new Tuple<Genome, Genome>(aGenome, bGenome), random);
            AgriPlant resultPlant = this.plantMutator.pickOrMutate(a.agriPlant, b.agriPlant, random);
            if(resultPlant == null || resultGenome == null) 
            {
                return false;
            }
            // Spawn the child
            return this.spawnChild(target, resultGenome, resultPlant);
        }
        
        // Default AgriCraft plant mutation logic
        public class AgriPlantMutator
        {
            public AgriPlantMutator() { }


            public AgriPlant pickOrMutate(AgriPlant first,
                                          AgriPlant second,
                                          Random random)
            //IAgriCrop crop, IAgriGene<IAgriPlant> gene, IAllele<IAgriPlant> first, IAllele<IAgriPlant> second,
            //                                        Tuple<IAgriGenome, IAgriGenome> parents, Random random)
            {

                // Search for matching mutations
                // get trigger results
                // filter out any forbidden results
                // order them randomly
                // fetch the first
                if (random.NextDouble() < 0.7)
                {
                    return random.Next() > 0.5
                        ? first.Cloneable && random.NextDouble() < first.SpreadChance *7  ? first : null
                        : (second.Cloneable && random.NextDouble() < second.SpreadChance *7) ? second : null;
                }
                
            
                var l = cancrops.GetMutations().getMutationsFromParents((first, second))
                        // get trigger results
                       // .ConvertAll(m => new Tuple<IAgriMutation, ConditionResult>(m, this.evaluateTriggers(crop, m)))
                        // filter out any forbidden results
                        //.Where(m => m.Item2 != IAgriMutation.ConditionResult.FORBID)
                        // Find the first result, sorted by trigger result, but shuffled by mutation
                        .ToList();
                

                l.Sort((a, b) => this.sortAndShuffle(a, b, random));
                if(l.Count() == 0)
                {
                    return null;
                }
                return l.First().getChild();
                //v//ar ag = evaluate(min, random);
                //var gen = gene.generateGenePair(gene.getAllele(ag), random.Next(1) > 0 ? first : second);
                //return gen == null ? gen : gene.generateGenePair(first, second);
                // map it to its child, or to nothing based on the mutation success rate
                //.flatMap(m-> this.evaluate(m, random))
                // map the result to a new gene pair with either of its parents as second gene
                //.map(plant->gene.generateGenePair(gene.getAllele(plant), random.Next(1) > 0 ? first : second))
                // if no mutation was found or if the mutation was unsuccessful, return a gene pair of the parents
                //.orElse(gene.generateGenePair(first, second));
            }
            /*protected IAgriMutation.ConditionResult evaluateTriggers(IAgriCrop crop, IAgriMutation mutation)
            {
                var l = mutation.getConditions()
                        .ConvertAll(trigger => trigger.getResult(crop, mutation));
                l.Sort();
                return l.Count > 0 ? l[0] : IAgriMutation.ConditionResult.PASS;
            }*/
            protected int sortAndShuffle(AgriMutation a, AgriMutation b, Random random)
            {
                return random.Next(1) > 0 ? -1 : 1;
            }

            /*protected AgriPlant evaluate(Tuple<IAgriMutation, IAgriMutation.ConditionResult> mutation, Random random)
            {
                // If the mutation is forced, no need to evaluate a probability
                if (mutation.Item2 == IAgriMutation.ConditionResult.FORCE)
                {
                    return mutation.Item1.getChild();
                }
                // Evaluate mutation probability
                if (mutation.Item1.getChance() > random.NextDouble())
                {
                    return mutation.Item1.getChild();
                }
                // mutation failed
                return null;
            }*/
        }

        // Default AgriCraft stat mutation logic
        public class AgriStatMutator
        {
            public AgriStatMutator() { }


            public Gene pickOrMutate(Gene gene, Tuple<Genome, Genome>  parents, Random random)
            {
                // return new gene pair with or without mutations, based on mutativity stat
                return new Gene(gene.StatName,
                        this.rollAndExecuteMutation(gene, parents.Item1.Mutativity.Dominant.Value , random),
                        this.rollAndExecuteMutation(gene, parents.Item2.Mutativity.Recessive.Value, random), gene.Hidden
                );
            }

            protected Allele rollAndExecuteMutation(Gene gene, int statValue, Random random)
            {
                // Mutativity stat of 1 results in 25/50/25 probability of positive/no/negative mutation
                // Mutativity stat of 10 results in 100/0/0 probability of positive/no/negative mutation
                int max = cancrops.config.maxMutativity;
                if (random.Next(max) < statValue)
                {
                    int delta = random.Next(max) < (max + statValue) / 2 ? 1 : -1;
                    int newValue = delta + gene.Dominant.Value;
                    if (newValue <= 0)
                    {
                        return new Allele(1);
                    }
                    else if (newValue > 10) //TODO add dict with max values
                    {
                        return new Allele(10);
                    }
                    return new Allele(newValue);
                    //return new Allele(gene.Dominant.Value + delta);
                }
                else
                {
                    return new Allele(gene.Dominant.Value);
                }
            }
        }
    }
}