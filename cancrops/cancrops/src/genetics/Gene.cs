using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cancrops.src.genetics
{
    // Represents a single genetic trait with dominant and recessive alleles.
    // In Mendelian genetics, the dominant allele determines the expressed trait (phenotype).
    // Each gene corresponds to a crop statistic (gain, growth, strength, resistance, fertility, mutativity).
    public class Gene
    {
        // The dominant allele - this value is used for the expressed trait
        public Allele Dominant;
		
        // The recessive allele - stored but only expressed if no dominant allele present
        public Allele Recessive;
		
        // Name of the stat this gene controls (e.g., "gain", "growth", "strength")
        public string StatName;
		
        // Whether this gene should be hidden in the UI
        public bool Hidden;
        
        public Gene(string statName, Allele D, Allele R) 
        { 
            this.StatName = statName;
            this.Dominant = D;
            this.Recessive = R;
        }
        
        // Creates a deep copy of this gene
        public Gene Clone()
        {
            return new Gene(this.StatName, new Allele(this.Dominant.Value), new Allele(this.Recessive.Value));
        }

    }
}
