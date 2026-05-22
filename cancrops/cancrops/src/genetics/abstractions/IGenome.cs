using System.Collections.Generic;
using Vintagestory.API.Datastructures;

namespace cancrops.src.genetics.abstractions
{
    public interface IGenome
    {
        IGenePair GetGenePair(IGene gene);
        IGenePair<T> GetGenePair<T>(IGene<T> gene);
        void SetGenePair(IGenePair pair);
        IEnumerable<IGenePair> AllPairs { get; }
        IGenome Clone();
        ITreeAttribute AsTreeAttribute();
    }
}
