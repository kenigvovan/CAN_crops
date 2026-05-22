using cancrops.src.genetics.abstractions;
using Vintagestory.API.Datastructures;

namespace cancrops.src.genetics.genes
{
    public class IntAllele : IAllele<int>
    {
        public IGene<int> Gene { get; }
        IGene IAllele.Gene => Gene;

        public int Trait { get; }
        public int ComparatorValue => Trait;

        public IntAllele(IGene<int> gene, int value)
        {
            Gene = gene;
            Trait = value;
        }

        public bool IsDominant(IAllele<int> other) => Trait >= other.Trait;

        public void WriteToTreeAttribute(ITreeAttribute tree)
        {
            tree.SetInt("V", Trait);
        }
    }
}
