using System;
using System.Collections.Generic;
using cancrops.src.BE;
using cancrops.src.genetics.abstractions;
using cancrops.src.implementations;

namespace cancrops.src.genetics
{
    public class AgriMutationHandler
    {
        private readonly ParentSelector selector;
        private readonly CrossBreedEngine engine;

        public AgriMutationHandler()
        {
            selector = new ParentSelector();
            engine = new CrossBreedEngine();
        }

        public bool handleCrossBreedTick(CANBECrossSticks crossSticks, IEnumerable<CANBECrop> neighbours, Random random)
        {
            List<CANBECrop> candidates = selector.selectAndOrder(neighbours, random);
            if (candidates.Count == 0) return false;
            if (candidates.Count == 1) return doClone(crossSticks, candidates[0], random);

            // Per-tick crossbreed throttle (config.crossBreedTickChance).
            if (random.NextDouble() < cancrops.config.crossBreedTickChance)
            {
                return doCombine(crossSticks, candidates[0], candidates[1], random);
            }
            return false;
        }

        private bool doClone(CANBECrossSticks target, CANBECrop parent, Random random)
        {
            AgriPlant plant = parent.agriPlant;
            if (plant == null || !plant.AllowCloning) return false;
            if (random.NextDouble() >= plant.SpreadChance) return false;

            IGenome childGenome = engine.Clone(parent.Genome, target.Pos, target.Api.World, random);
            return spawnChild(target, childGenome);
        }

        private bool doCombine(CANBECrossSticks target, CANBECrop a, CANBECrop b, Random random)
        {
            if (a.Genome == null || b.Genome == null) return false;
            IGenome childGenome = engine.Combine(a.Genome, b.Genome, target.Pos, target.Api.World, random);
            return spawnChild(target, childGenome);
        }

        private bool spawnChild(CANBECrossSticks target, IGenome childGenome)
        {
            AgriPlant childPlant = CrossBreedEngine.GetSpecies(childGenome);
            if (childPlant == null) return false;
            // childGenome is a MapGenome built by the engine; wrap into legacy Genome for spawn API
            var legacy = childGenome is MapGenome map ? new Genome(map) : null;
            if (legacy == null) return false;
            return target.spawnGenome(legacy, childPlant);
        }
    }
}
