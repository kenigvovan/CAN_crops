using Vintagestory.API.Datastructures;

namespace cancrops.src.genetics.abstractions
{
    public interface IGenePair
    {
        IGene Gene { get; }
        IGenePair Clone();
        void WriteToTreeAttribute(ITreeAttribute tree);
    }

    public interface IGenePair<T> : IGenePair
    {
        new IGene<T> Gene { get; }
        IAllele<T> Dominant { get; }
        IAllele<T> Recessive { get; }
        T GetTrait();
        new IGenePair<T> Clone();
    }
}
