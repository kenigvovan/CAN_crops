using System;
using System.Collections;
using System.Collections.Generic;
using Vintagestory.API.Datastructures;

namespace cancrops.src.genetics
{
    // Represents a complete set of genetic information for a crop plant.
    // Contains 6 genes that control different aspects of the crop's behavior:
    // - Gain: Amount of drops when harvested
    // - Growth: Speed of growth (reduces time between stages)
    // - Strength: Affects perish time of harvested items
    // - Resistance: Temperature tolerance and weed resistance
    // - Fertility: Probability of being selected as a parent during breeding
    // - Mutativity: Chance of genetic mutations during breeding
    public class Genome: IEnumerable<Gene>
    {
        // Lazy initialization to avoid static initialization order issues
        // This dictionary tracks which genes should be hidden in the UI
        private static Dictionary<string, bool> _genes;
        public static Dictionary<string, bool> genes
        {
            get
            {
                if (_genes == null)
                {
                    _genes = new Dictionary<string, bool>
                    {
                        {"gain", cancrops.config?.hiddenGain ?? false},
                        {"growth", cancrops.config?.hiddenGrowth ?? false},
                        {"strength", cancrops.config?.hiddenStrength ?? false},
                        {"resistance", cancrops.config?.hiddenResistance ?? false},
                        {"fertility", cancrops.config?.hiddenFertility ?? true},
                        {"mutativity", cancrops.config?.hiddenMutativity ?? true}
                    };
                }
                return _genes;
            }
        }        
        public Gene Gain { get; set; }
        public Gene Growth { get; set; }
        public Gene Strength { get; set; }
        public Gene Resistance { get; set; }
        public Gene Fertility { get; set; }
        public Gene Mutativity { get; set; }
        public Genome()
        {
            Gain = new Gene("gain", new Allele(cancrops.config.minGain), new Allele(cancrops.config.minGain));
            Growth = new Gene("growth", new Allele(cancrops.config.minGrowth), new Allele(cancrops.config.minGrowth));
            Strength = new Gene("strength", new Allele(cancrops.config.minStrength), new Allele(cancrops.config.minStrength));
            Resistance = new Gene("resistance", new Allele(cancrops.config.minResistance), new Allele(cancrops.config.minResistance));
            Fertility = new Gene("fertility", new Allele(cancrops.config.minFertility), new Allele(cancrops.config.minFertility));
            Mutativity = new Gene("mutativity", new Allele(cancrops.config.minMutativity), new Allele(cancrops.config.minMutativity));
        }
        public Genome(Gene gain, Gene growth, Gene strength, Gene resistance, Gene fertility, Gene mutativity)
        {
            Gain = gain;
            Growth = growth;
            Strength = strength;
            Resistance = resistance;
            Fertility = fertility;
            Mutativity = mutativity;
        }
        public Genome(List<Gene> genes)
        {
            Gain = genes[0];
            Growth = genes[1];
            Strength = genes[2];
            Resistance = genes[3];
            Fertility = genes[4];
            Mutativity = genes[5];
        }
        public Genome Clone()
        {
            Genome newGenome = new Genome();
            newGenome.Gain.Dominant.Value = this.Gain.Dominant.Value;
            newGenome.Gain.Recessive.Value = this.Gain.Recessive.Value;

            newGenome.Growth.Dominant.Value = this.Growth.Dominant.Value;
            newGenome.Growth.Recessive.Value = this.Growth.Recessive.Value;

            newGenome.Strength.Dominant.Value = this.Strength.Dominant.Value;
            newGenome.Strength.Recessive.Value = this.Strength.Recessive.Value;

            newGenome.Resistance.Dominant.Value = this.Resistance.Dominant.Value;
            newGenome.Resistance.Recessive.Value = this.Resistance.Recessive.Value;

            newGenome.Fertility.Dominant.Value = this.Fertility.Dominant.Value;
            newGenome.Fertility.Recessive.Value = this.Fertility.Recessive.Value;

            newGenome.Mutativity.Dominant.Value = this.Mutativity.Dominant.Value;
            newGenome.Mutativity.Recessive.Value = this.Mutativity.Recessive.Value;
            return newGenome;
        }
        public Gene Clone(Gene gene)
        {
            foreach(var g in this.AsEnumerable())
            {
                if(g.StatName == gene.StatName)
                {
                    return g.Clone();
                }
            }
            return null;
        }
        public Gene GetGeneByName(string name)
        {
            foreach(var it in this.AsEnumerable())
            {
                if(it.StatName.Equals(name))
                {
                    return it;
                }
            }
            return null;
        }
        public bool SetGene(string name, Gene value)
        {
            if (name.Equals("gain"))
            {
                Gain = value;
                return true;
            }
            else if (name.Equals("growth"))
            {
                Growth = value;
                return true;
            }
            else if (name.Equals("strength"))
            {
                Strength = value;
                return true;
            }
            else if (name.Equals("resistance"))
            {
                Resistance = value;
                return true;
            }
            else if (name.Equals("fertility"))
            {
                Fertility = value;
                return true;
            }
            else if (name.Equals("mutativity"))
            {
                Mutativity = value;
                return true;
            }
            return false;
        }
        public ITreeAttribute AsTreeAttribute()
        {
            ITreeAttribute newTree = new TreeAttribute();
            foreach(var gene in this.AsEnumerable())
            {
                ITreeAttribute geneTree = new TreeAttribute();
                geneTree.SetString("name", gene.StatName);
                geneTree.SetInt("D", gene.Dominant.Value);
                geneTree.SetInt("R", gene.Recessive.Value);
                //geneTree.SetBool("H", gene.Hidden);
                newTree[gene.StatName] = geneTree;
            }
            return newTree;
        }
        public static Genome FromTreeAttribute(ITreeAttribute tree)
        {
            if(tree == null)
            {
                return null;
            }
            List<Gene> newGenes = new List<Gene>();
            foreach(var geneName in genes.Keys)
            {
                ITreeAttribute geneTree = tree.GetTreeAttribute(geneName);
                
                newGenes.Add(new Gene(geneName, new Allele(geneTree.GetInt("D")), new Allele(geneTree.GetInt("R"))));
            }
            return new Genome(newGenes);
        }
        public IEnumerator<Gene> GetEnumerator()
        {
            yield return Gain;
            yield return Growth;
            yield return Strength;
            yield return Resistance;
            yield return Fertility;
            yield return Mutativity;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
