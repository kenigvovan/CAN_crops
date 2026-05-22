using System;
using System.Collections.Generic;
using cancrops.src.implementations;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace cancrops.src.genetics.abstractions
{
    public interface IGene
    {
        string Id { get; }
        bool IsHidden { get; }
        IGenePair GenerateDefaultPair(AgriPlant plant);
        IGenePair ReadPairFromTreeAttribute(ITreeAttribute tree);

        // Picks one random allele from each parent pair, runs them through the gene's mutator,
        // and returns the offspring's pair. Type-specific dispatch happens inside the gene.
        IGenePair CrossBreed(
            IGenePair pairA,
            IGenePair pairB,
            IGenome parentA,
            IGenome parentB,
            BlockPos pos,
            IWorldAccessor world,
            Random random);
    }

    public interface IGene<T> : IGene
    {
        IAllele<T> DefaultAllele(AgriPlant plant);
        IAllele<T> GetAllele(T value);
        IEnumerable<IAllele<T>> AllAlleles { get; }
        IMutator<T> Mutator { get; }
        IGenePair<T> GeneratePair(IAllele<T> first, IAllele<T> second);
        IAllele<T> ReadAlleleFromTreeAttribute(ITreeAttribute tree);
    }
}
