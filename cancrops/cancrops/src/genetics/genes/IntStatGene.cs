using System;
using System.Collections.Generic;
using cancrops.src.genetics.abstractions;
using cancrops.src.implementations;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace cancrops.src.genetics.genes
{
    public class IntStatGene : IGene<int>
    {
        public string Id { get; }
        public bool IsHidden { get; }
        public int Min { get; }
        public int Max { get; }
        public IMutator<int> Mutator { get; }

        private readonly Dictionary<int, IntAllele> alleleByValue = new Dictionary<int, IntAllele>();

        public IEnumerable<IAllele<int>> AllAlleles => alleleByValue.Values;

        public IntStatGene(string id, int min, int max, bool isHidden, IMutator<int> mutator)
        {
            Id = id;
            Min = min;
            Max = max;
            IsHidden = isHidden;
            Mutator = mutator;
            for (int v = min; v <= max; v++)
            {
                alleleByValue[v] = new IntAllele(this, v);
            }
        }

        public IAllele<int> DefaultAllele(AgriPlant plant) => alleleByValue[Min];

        public IAllele<int> GetAllele(int value)
        {
            int clamped = value < Min ? Min : (value > Max ? Max : value);
            return alleleByValue[clamped];
        }

        public IGenePair<int> GeneratePair(IAllele<int> first, IAllele<int> second)
        {
            return new IntGenePair(this, first, second);
        }

        public IAllele<int> ReadAlleleFromTreeAttribute(ITreeAttribute tree)
        {
            return GetAllele(tree.GetInt("V"));
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
            var typedA = (IGenePair<int>)pairA;
            var typedB = (IGenePair<int>)pairB;
            var alleleFromA = random.Next(2) == 0 ? typedA.Dominant : typedA.Recessive;
            var alleleFromB = random.Next(2) == 0 ? typedB.Dominant : typedB.Recessive;
            return Mutator.PickOrMutate(this, alleleFromA, alleleFromB, (parentA, parentB), pos, world, random);
        }
    }
}
