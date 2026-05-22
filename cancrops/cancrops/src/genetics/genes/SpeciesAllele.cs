using cancrops.src.genetics.abstractions;
using cancrops.src.implementations;
using Vintagestory.API.Datastructures;

namespace cancrops.src.genetics.genes
{
    public class SpeciesAllele : IAllele<AgriPlant>
    {
        public IGene<AgriPlant> Gene { get; }
        IGene IAllele.Gene => Gene;

        public AgriPlant Trait { get; }
        public string PlantKey { get; }

        public int ComparatorValue => PlantKey.GetHashCode();

        public SpeciesAllele(IGene<AgriPlant> gene, AgriPlant plant)
        {
            Gene = gene;
            Trait = plant;
            PlantKey = plant.Domain + ":" + plant.Id;
        }

        public bool IsDominant(IAllele<AgriPlant> other)
        {
            if (other is SpeciesAllele otherSpecies)
            {
                int myComplexity = cancrops.GetMutations()?.Complexity(Trait) ?? 0;
                int otherComplexity = cancrops.GetMutations()?.Complexity(otherSpecies.Trait) ?? 0;
                if (myComplexity != otherComplexity) return myComplexity > otherComplexity;
                // Tiebreak: lexicographic on plant key, deterministic and stable.
                return string.Compare(PlantKey, otherSpecies.PlantKey) <= 0;
            }
            return true;
        }

        public void WriteToTreeAttribute(ITreeAttribute tree)
        {
            tree.SetString("plant", PlantKey);
        }
    }
}
