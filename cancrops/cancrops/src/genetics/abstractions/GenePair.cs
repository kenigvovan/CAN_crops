using Vintagestory.API.Datastructures;

namespace cancrops.src.genetics.abstractions
{
    public class GenePair<T> : IGenePair<T>
    {
        public IGene<T> Gene { get; }
        IGene IGenePair.Gene => Gene;

        public IAllele<T> Dominant { get; }
        public IAllele<T> Recessive { get; }

        public GenePair(IGene<T> gene, IAllele<T> first, IAllele<T> second)
        {
            Gene = gene;
            if (first.IsDominant(second))
            {
                Dominant = first;
                Recessive = second;
            }
            else
            {
                Dominant = second;
                Recessive = first;
            }
        }

        public virtual T GetTrait() => Dominant.Trait;

        public virtual IGenePair<T> Clone() => Gene.GeneratePair(Dominant, Recessive);
        IGenePair IGenePair.Clone() => Clone();

        public void WriteToTreeAttribute(ITreeAttribute tree)
        {
            tree.SetString("geneId", Gene.Id);
            ITreeAttribute dom = new TreeAttribute();
            ITreeAttribute rec = new TreeAttribute();
            Dominant.WriteToTreeAttribute(dom);
            Recessive.WriteToTreeAttribute(rec);
            tree["D"] = dom;
            tree["R"] = rec;
        }
    }
}
