using System;
using System.Collections.Generic;
using cancrops.src.genetics.abstractions;
using cancrops.src.implementations;
using cancrops.src.utility;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace cancrops.src.genetics.genes
{
    public class SpeciesGene : IGene<AgriPlant>
    {
        public const string GENE_ID = "species";

        public string Id => GENE_ID;
        public bool IsHidden => false;
        public IMutator<AgriPlant> Mutator { get; }

        private readonly Dictionary<string, SpeciesAllele> alleleByKey = new Dictionary<string, SpeciesAllele>();
        private readonly AgriPlants plantsRegistry;

        public IEnumerable<IAllele<AgriPlant>> AllAlleles => alleleByKey.Values;

        public SpeciesGene(AgriPlants plantsRegistry, IEnumerable<AgriPlant> plants, IMutator<AgriPlant> mutator)
        {
            this.plantsRegistry = plantsRegistry;
            Mutator = mutator;
            foreach (var plant in plants)
            {
                var allele = new SpeciesAllele(this, plant);
                alleleByKey[allele.PlantKey] = allele;
            }
        }

        public IAllele<AgriPlant> DefaultAllele(AgriPlant plant)
        {
            return GetAllele(plant);
        }

        public IAllele<AgriPlant> GetAllele(AgriPlant plant)
        {
            if (plant == null) return null;
            string key = plant.Domain + ":" + plant.Id;
            return alleleByKey.TryGetValue(key, out var allele) ? allele : null;
        }

        public IAllele<AgriPlant> GetAlleleByKey(string plantKey)
        {
            return alleleByKey.TryGetValue(plantKey, out var allele) ? allele : null;
        }

        public IGenePair<AgriPlant> GeneratePair(IAllele<AgriPlant> first, IAllele<AgriPlant> second)
        {
            return new GenePair<AgriPlant>(this, first, second);
        }

        public IAllele<AgriPlant> ReadAlleleFromTreeAttribute(ITreeAttribute tree)
        {
            string key = tree.GetString("plant");
            return GetAlleleByKey(key);
        }

        public IGenePair GenerateDefaultPair(AgriPlant plant)
        {
            var d = DefaultAllele(plant);
            return GeneratePair(d, d);
        }

        public IGenePair ReadPairFromTreeAttribute(ITreeAttribute tree)
        {
            var dom = ReadAlleleFromTreeAttribute(tree.GetTreeAttribute("D"));
            var rec = ReadAlleleFromTreeAttribute(tree.GetTreeAttribute("R"));
            return GeneratePair(dom, rec);
        }

        public IGenePair CrossBreed(IGenePair pairA, IGenePair pairB, IGenome parentA, IGenome parentB, BlockPos pos, IWorldAccessor world, Random random)
        {
            var typedA = (IGenePair<AgriPlant>)pairA;
            var typedB = (IGenePair<AgriPlant>)pairB;
            var alleleFromA = random.Next(2) == 0 ? typedA.Dominant : typedA.Recessive;
            var alleleFromB = random.Next(2) == 0 ? typedB.Dominant : typedB.Recessive;
            return Mutator.PickOrMutate(this, alleleFromA, alleleFromB, (parentA, parentB), pos, world, random);
        }
    }
}
