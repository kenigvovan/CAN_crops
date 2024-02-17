using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cancrops.src.utility
{
    public interface IAgriRegisterable : IComparable
    {

        /**
         * The unique id of the AgriRegisterable.
         *
         * @return the unique id of the element.
         */
        public abstract string getId();
    }
}
