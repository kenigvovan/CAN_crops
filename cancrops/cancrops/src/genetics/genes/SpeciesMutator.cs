using System;
using System.Collections.Generic;
using cancrops.src.genetics.abstractions;
using cancrops.src.genetics.conditions;
using cancrops.src.implementations;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace cancrops.src.genetics.genes
{
    public class SpeciesMutator : IMutator<AgriPlant>
    {
        public IGenePair<AgriPlant> PickOrMutate(
            IGene<AgriPlant> gene,
            IAllele<AgriPlant> first,
            IAllele<AgriPlant> second,
            (IGenome a, IGenome b) parents,
            BlockPos pos,
            IWorldAccessor world,
            Random random)
        {
            AgriPlant plantA = first.Trait;
            AgriPlant plantB = second.Trait;

            AgriMutation chosen = TryFindMutation(plantA, plantB, world, pos, random);
            if (chosen != null && gene is SpeciesGene speciesGene)
            {
                AgriPlant childPlant = chosen.getChild();
                if (childPlant != null)
                {
                    var childAllele = speciesGene.GetAllele(childPlant);
                    if (childAllele != null)
                    {
                        var passthrough = random.Next(2) == 0 ? first : second;
                        return gene.GeneratePair(childAllele, passthrough);
                    }
                }
            }

            return gene.GeneratePair(first, second);
        }

        private AgriMutation TryFindMutation(AgriPlant a, AgriPlant b, IWorldAccessor world, BlockPos pos, Random random)
        {
            if (a == null || b == null) return null;

            var forced = new List<AgriMutation>();
            var rolled = new List<AgriMutation>();

            foreach (var mutation in cancrops.GetMutations().getMutationsFromParents((a, b)))
            {
                var verdict = EvaluateConditions(mutation.Conditions, world, pos);
                if (verdict == EnumConditionResult.FORBID) continue;
                if (verdict == EnumConditionResult.FORCE) { forced.Add(mutation); continue; }
                if (mutation.randomMutate(random)) rolled.Add(mutation);
            }

            // Forced wins over rolled. Among each tier, pick uniformly at random.
            if (forced.Count > 0) return forced[random.Next(forced.Count)];
            if (rolled.Count > 0) return rolled[random.Next(rolled.Count)];
            return null;
        }

        // Combine multiple conditions:
        //   any FORBID → FORBID
        //   any FORCE  → FORCE (unless any FORBID also)
        //   else       → PASS
        private static EnumConditionResult EvaluateConditions(List<IMutationCondition> conditions, IWorldAccessor world, BlockPos pos)
        {
            if (conditions == null || conditions.Count == 0) return EnumConditionResult.PASS;
            bool anyForce = false;
            foreach (var cond in conditions)
            {
                var r = cond.Check(world, pos);
                if (r == EnumConditionResult.FORBID) return EnumConditionResult.FORBID;
                if (r == EnumConditionResult.FORCE) anyForce = true;
            }
            return anyForce ? EnumConditionResult.FORCE : EnumConditionResult.PASS;
        }
    }
}
