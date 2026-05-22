using cancrops.src.implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cancrops.src.utility
{
    public class AgriMutations
    {
        public Dictionary<(string, string), List<AgriMutation>> mutationListByParents;

        // Plant id ("domain:name") → minimum complexity (= length of shortest mutation chain
        // back to wild parents). Wild plants (no mutation produces them) have complexity 0.
        // Used by SpeciesAllele.IsDominant to favour bred over wild traits.
        private Dictionary<string, int> complexityByPlantKey = new Dictionary<string, int>();
        private bool complexityBuilt = false;
        private readonly object complexityLock = new object();

        public AgriMutations()
        {
            mutationListByParents = new Dictionary<(string, string), List<AgriMutation>> (new MutationParentsTupleComparer());
        }
        public bool AddMutation(AgriMutation mutation)
        {
            lock (complexityLock) { complexityBuilt = false; }
            mutationListByParents.TryGetValue((mutation.Parent1, mutation.Parent2), out List<AgriMutation>  mutations);
            if(mutations == null)
            {
                mutationListByParents.Add((mutation.Parent1, mutation.Parent2),
                    new List<AgriMutation> { mutation });
                return true;
            }
            else
            {
                if(mutations.Contains(mutation))
                {
                    return false;
                }
                else
                {
                    mutations.Add(mutation);
                    return true;
                }
            }
        }
        public IEnumerable<AgriMutation> getMutationsFromParents((AgriPlant, AgriPlant) plants)
        {
            this.mutationListByParents.TryGetValue((plants.Item1.Domain + ":" + plants.Item1.Id, plants.Item2.Domain + ":" + plants.Item2.Id), out List<AgriMutation> mutations);
            return mutations ?? new List<AgriMutation>();
        }

        public int Complexity(AgriPlant plant)
        {
            if (plant == null) return 0;
            lock (complexityLock)
            {
                if (!complexityBuilt) BuildComplexityMapLocked();
                string key = plant.Domain + ":" + plant.Id;
                return complexityByPlantKey.TryGetValue(key, out int c) ? c : 0;
            }
        }

        // Computes complexity for every plant that appears as a child of any mutation.
        // Plants never produced by a mutation are absent from the map and resolve to 0 (wild).
        public void BuildComplexityMap()
        {
            lock (complexityLock)
            {
                BuildComplexityMapLocked();
            }
        }

        private void BuildComplexityMapLocked()
        {
            complexityByPlantKey.Clear();
            var children = new HashSet<string>();
            foreach (var list in mutationListByParents.Values)
            {
                foreach (var m in list) children.Add(m.Child);
            }
            foreach (var childKey in children)
            {
                ComputeComplexity(childKey, new HashSet<string>());
            }
            complexityBuilt = true;
        }

        // Memoised complexity for a single plant key. visitingPath breaks cycles by treating
        // re-entrant nodes as wild (complexity 0).
        private int ComputeComplexity(string plantKey, HashSet<string> visitingPath)
        {
            if (complexityByPlantKey.TryGetValue(plantKey, out int cached)) return cached;
            if (visitingPath.Contains(plantKey)) return 0;

            visitingPath.Add(plantKey);
            int minComplexity = int.MaxValue;
            bool found = false;
            foreach (var list in mutationListByParents.Values)
            {
                foreach (var m in list)
                {
                    if (m.Child != plantKey) continue;
                    int c = ComputeComplexity(m.Parent1, visitingPath)
                          + ComputeComplexity(m.Parent2, visitingPath)
                          + 1;
                    if (c < minComplexity) { minComplexity = c; found = true; }
                }
            }
            visitingPath.Remove(plantKey);

            int result = found ? minComplexity : 0;
            complexityByPlantKey[plantKey] = result;
            return result;
        }
    }
}
