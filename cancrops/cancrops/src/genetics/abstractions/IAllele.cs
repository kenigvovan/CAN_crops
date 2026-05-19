using Vintagestory.API.Datastructures;

namespace cancrops.src.genetics.abstractions
{
    public interface IAllele
    {
        IGene Gene { get; }
        int ComparatorValue { get; }
        void WriteToTreeAttribute(ITreeAttribute tree);
    }

    public interface IAllele<T> : IAllele
    {
        new IGene<T> Gene { get; }
        T Trait { get; }
        bool IsDominant(IAllele<T> other);
    }
}
