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
        public AgriMutations()
        {
            mutationListByParents = new Dictionary<(string, string), List<AgriMutation>> (new MutationParentsTupleComparer());
        }
        public bool AddMutation(AgriMutation mutation)
        {
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
    }
}
