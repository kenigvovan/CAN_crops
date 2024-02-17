using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cancrops.src.genetics
{
    public class Gene
    {
        public Allele Dominant;
        public Allele Recessive;
        public string StatName;
        public bool Hidden;
        public Gene(string statName, Allele D, Allele R, bool Hidden) 
        { 
            this.StatName = statName;
            this.Dominant = D;
            this.Recessive = R;
            this.Hidden = Hidden;
        }
        public Gene Clone()
        {
            return new Gene(this.StatName, new Allele(this.Dominant.Value), new Allele(this.Recessive.Value), this.Hidden);
        }

    }
}
