using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cancrops.src.utility
{
    public class MutationParentsTupleComparer : IEqualityComparer<(string, string)>
    {
        public bool Equals((string, string) x, (string, string) y)
        {
            return (x.Item1 == y.Item1 && x.Item2 == y.Item2) || (x.Item1 == y.Item2 && x.Item2 == y.Item1);
        }

        public int GetHashCode((string, string) obj)
        {
            int hash = 17;
            hash = hash * 23 + obj.Item1.GetHashCode();
            hash = hash * 23 + obj.Item2.GetHashCode();
            return hash;
        }
    }
}
