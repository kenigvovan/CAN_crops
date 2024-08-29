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
            bool firstHigher = obj.Item1.CompareTo(obj.Item2) > 0;
            int hash = 17;
            hash = hash * 23 + (firstHigher ? obj.Item1.GetHashCode() : obj.Item2.GetHashCode());
            hash = hash * 23 + (firstHigher ? obj.Item2.GetHashCode() : obj.Item1.GetHashCode());
            return hash;
        }
    }
}
