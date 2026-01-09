
using cancrops.src.genetics;
using cancrops.src.implementations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace cancrops.src.utility
{
    public static class CommonUtils
    {
        public static Color white = Color.FromName("white");
        public static bool tryFindColor(string inColorString, out int resColor)
        {
            Color clr = Color.FromName(inColorString);
            if (!clr.IsKnownColor)
            {
                resColor = Color.White.ToArgb();
                return false;
            }
            resColor = clr.ToArgb();
            return true;
        }
        public static Genome GetSeedGenomeFromAttribute(ItemStack seedStack)
        {
            ITreeAttribute genomeTree = seedStack.Attributes.GetTreeAttribute(cancrops.config.genome_tag);
            if (genomeTree == null)
            {
                //return default genome
                return new Genome();
            }
            else
            {
                List<Gene> genes = new List<Gene>();
                foreach (var it in Genome.genes)
                {
                    ITreeAttribute geneTree = genomeTree.GetTreeAttribute(it.Key);
                    genes.Add(new Gene(it.Key, new Allele(geneTree.GetInt("D")), new Allele(geneTree.GetInt("R"))));
                }
                return new Genome(genes);
            }
        }
        public static ItemStack GetSeedItemStackFromFarmland(Genome genome, AgriPlant agriPlant)
        {
            ItemStack stack = new ItemStack(cancrops.sapi.World.GetItem(new AssetLocation(agriPlant.Domain + ":seeds-" + agriPlant.Id)), 1);
            ITreeAttribute genomeTree = new TreeAttribute();
            foreach (Gene gene in genome)
            {
                ITreeAttribute geneTree = new TreeAttribute();
                geneTree.SetInt("D", gene.Dominant.Value);
                geneTree.SetInt("R", gene.Recessive.Value);
                genomeTree[gene.StatName] = geneTree;
            }
            stack.Attributes[cancrops.config.genome_tag] = genomeTree;
            return stack;
        }
        public static void ApplyGenomeTreeToItemstack(Genome genome, ItemStack itemStack)
        {
            ITreeAttribute genomeTree = new TreeAttribute();
            foreach (Gene gene in genome)
            {
                ITreeAttribute geneTree = new TreeAttribute();
                geneTree.SetInt("D", gene.Dominant.Value);
                geneTree.SetInt("R", gene.Recessive.Value);
                genomeTree[gene.StatName] = geneTree;
            }
            itemStack.Attributes[cancrops.config.genome_tag] = genomeTree;
        }
        internal static bool MergeGenomesInnerMean(List<Genome> genomeList, out Genome genome)
        {
            Genome newGenome = new Genome();
            if(cancrops.config.seedMergeStrategy == "mean")
            {
                foreach (var geneName in Genome.genes.Keys)
                {
                    int Dvalue = 0;
                    int Rvalue = 0;
                    foreach (var presentGenome in genomeList)
                    {
                        var geneValues = presentGenome.GetGeneByName(geneName);
                        Dvalue += geneValues.Dominant.Value;
                        Rvalue += geneValues.Recessive.Value;
                    }
					Dvalue = (int)Math.Round((double)Dvalue / genomeList.Count, MidpointRounding.AwayFromZero);
					Rvalue = (int)Math.Round((double)Rvalue / genomeList.Count, MidpointRounding.AwayFromZero);
                    newGenome.SetGene(geneName, new Gene(geneName, new Allele(Dvalue), new Allele(Rvalue)));
                }
                genome = newGenome;
                return true;
            }
            if(cancrops.config.seedMergeStrategy == "tolower")
            {
                foreach (var geneName in Genome.genes.Keys)
                {
                    int Dvalue = -1;
                    int Rvalue = -1;
                    foreach (var presentGenome in genomeList)
                    {
                        var geneValues = presentGenome.GetGeneByName(geneName);
                        if (Dvalue == -1 || geneValues.Dominant.Value < Dvalue)
                        {
                            Dvalue = geneValues.Dominant.Value;
                        }
                        if (Rvalue == -1 || geneValues.Recessive.Value < Rvalue)
                        {
                            Rvalue = geneValues.Recessive.Value;
                        }
                    }
                    newGenome.SetGene(geneName, new Gene(geneName, new Allele(Dvalue), new Allele(Rvalue)));
                }
                genome = newGenome;
                return true;
            }
            genome = null;
            return false;
        }
        public static bool MergeGenomes(List<Genome> genomeList, out Genome genome)
        {
            if(MergeGenomesInnerMean(genomeList, out genome))
            {
                return true;
            }
            return false;
        }
    }
}
