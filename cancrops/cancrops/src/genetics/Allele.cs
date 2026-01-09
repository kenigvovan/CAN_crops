using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cancrops.src.genetics
{
    // Represents a single allele (variant of a gene) with an integer value from 0-10.
    // Higher values generally indicate better performance for that trait.
    public class Allele
    {
        public int Value;
        
        public Allele(int Val)
        {
            Value = Val;
        }
    }
}
