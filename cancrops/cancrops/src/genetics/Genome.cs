using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Datastructures;

namespace cancrops.src.genetics
{
    public class Genome: IEnumerable<Gene>
    {
        public static Dictionary<string, bool> genes = new Dictionary<string, bool>{ 
            //grow speed
                {"gain", true}, 
            //amount of drop
                {"growth", true},
            //resist against requirements
                {"strength", true},
            //against weeds
                {"resistance", true},  
            //chance to be selected from parents
                {"fertility", false},
            //
                {"mutativity", false}
        };        
        public Gene Gain { get; set; }
        public Gene Growth { get; set; }
        public Gene Strength { get; set; }
        public Gene Resistance { get; set; }
        public Gene Fertility { get; set; }
        public Gene Mutativity { get; set; }
        public Genome()
        {
            Gain = new Gene("gain", new Allele(cancrops.config.minGain), new Allele(cancrops.config.minGain), cancrops.config.hiddenGain);
            Growth = new Gene("growth", new Allele(cancrops.config.minGrowth), new Allele(cancrops.config.minGrowth), cancrops.config.hiddenGrowth);
            Strength = new Gene("strength", new Allele(cancrops.config.minStrength), new Allele(cancrops.config.minStrength), cancrops.config.hiddenStrength);
            Resistance = new Gene("resistance", new Allele(cancrops.config.minResistance), new Allele(cancrops.config.minResistance), cancrops.config.hiddenResistance);
            Fertility = new Gene("fertility", new Allele(cancrops.config.minFertility), new Allele(cancrops.config.minFertility), cancrops.config.hiddenFertility);
            Mutativity = new Gene("mutativity", new Allele(cancrops.config.minMutativity), new Allele(cancrops.config.minMutativity), cancrops.config.hiddenMutativity);
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
        public ITreeAttribute AsTreeAttribute()
        {
            ITreeAttribute newTree = new TreeAttribute();
            foreach(var gene in this.AsEnumerable())
            {
                ITreeAttribute geneTree = new TreeAttribute();
                geneTree.SetString("name", gene.StatName);
                geneTree.SetInt("D", gene.Dominant.Value);
                geneTree.SetInt("R", gene.Recessive.Value);
                geneTree.SetBool("H", gene.Hidden);
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
                
                newGenes.Add(new Gene(geneName, new Allele(geneTree.GetInt("D")), new Allele(geneTree.GetInt("R")), geneTree.GetBool("H")));
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
