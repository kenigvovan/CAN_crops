
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
                    genes.Add(new Gene(it.Key, new Allele(geneTree.GetInt("D")), new Allele(geneTree.GetInt("R")), it.Value));
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
    }
}
