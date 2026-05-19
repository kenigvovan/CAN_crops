using System.Collections.Generic;
using cancrops.src.genetics.abstractions;
using cancrops.src.genetics.genes;
using cancrops.src.implementations;
using Vintagestory.API.Datastructures;

namespace cancrops.src.genetics
{
    // Dictionary-backed genome implementation.
    // Reads new NBT format directly. Old-format saves are migrated in FromTreeAttribute when a
    // fallback plant is provided (used to seed the species gene as a homozygote).
    public class MapGenome : IGenome
    {
        public const int FORMAT_VERSION = 1;
        private const string KEY_VERSION = "version";
        private const string KEY_GENES = "genes";

        private readonly Dictionary<string, IGenePair> pairsByGeneId = new Dictionary<string, IGenePair>();

        public IEnumerable<IGenePair> AllPairs => pairsByGeneId.Values;

        public IGenePair GetGenePair(IGene gene)
        {
            return pairsByGeneId.TryGetValue(gene.Id, out var pair) ? pair : null;
        }

        public IGenePair<T> GetGenePair<T>(IGene<T> gene)
        {
            return GetGenePair((IGene)gene) as IGenePair<T>;
        }

        public void SetGenePair(IGenePair pair)
        {
            pairsByGeneId[pair.Gene.Id] = pair;
        }

        public IGenome Clone()
        {
            var copy = new MapGenome();
            foreach (var pair in pairsByGeneId.Values)
            {
                copy.SetGenePair(pair.Clone());
            }
            return copy;
        }

        public ITreeAttribute AsTreeAttribute()
        {
            ITreeAttribute root = new TreeAttribute();
            root.SetInt(KEY_VERSION, FORMAT_VERSION);
            ITreeAttribute genesTree = new TreeAttribute();
            foreach (var kv in pairsByGeneId)
            {
                ITreeAttribute pairTree = new TreeAttribute();
                kv.Value.WriteToTreeAttribute(pairTree);
                genesTree[kv.Key] = pairTree;
            }
            root[KEY_GENES] = genesTree;
            return root;
        }

        public static bool IsNewFormat(ITreeAttribute tree)
        {
            return tree != null && tree.HasAttribute(KEY_VERSION);
        }

        // Reads new-format NBT directly, or migrates old-format (legacy 5-stat tree) when given a fallbackPlant.
        public static MapGenome FromTreeAttribute(ITreeAttribute tree, AgriPlant fallbackPlant = null)
        {
            if (tree == null) return null;
            if (IsNewFormat(tree)) return FromNewFormat(tree);
            return MigrateFromLegacy(tree, fallbackPlant);
        }

        private static MapGenome FromNewFormat(ITreeAttribute tree)
        {
            var genome = new MapGenome();
            ITreeAttribute genesTree = tree.GetTreeAttribute(KEY_GENES);
            if (genesTree == null) return genome;

            foreach (var kv in genesTree)
            {
                IGene gene = GeneRegistry.Get(kv.Key);
                if (gene == null) continue; // unknown gene id — skip (mod removed?)
                ITreeAttribute pairTree = genesTree.GetTreeAttribute(kv.Key);
                if (pairTree == null) continue;
                var pair = gene.ReadPairFromTreeAttribute(pairTree);
                if (pair != null)
                {
                    genome.SetGenePair(pair);
                }
            }
            return genome;
        }

        // Old format: a TreeAttribute whose keys are stat names ("gain", "growth", ...) each
        // containing { "D": int, "R": int }. The species was tracked outside the genome; we
        // inject it here as a homozygote of fallbackPlant. "mutativity" is intentionally dropped.
        private static MapGenome MigrateFromLegacy(ITreeAttribute tree, AgriPlant fallbackPlant)
        {
            var genome = new MapGenome();

            foreach (var g in GeneRegistry.AllGenes)
            {
                if (!(g is IntStatGene intGene)) continue;
                ITreeAttribute geneTree = tree.GetTreeAttribute(intGene.Id);
                if (geneTree == null)
                {
                    // missing in legacy → take default
                    genome.SetGenePair(intGene.GenerateDefaultPair(fallbackPlant));
                    continue;
                }
                int d = geneTree.GetInt("D");
                int r = geneTree.GetInt("R");
                var pair = intGene.GeneratePair(intGene.GetAllele(d), intGene.GetAllele(r));
                genome.SetGenePair(pair);
            }

            // Inject species as homozygote from fallback plant, if available.
            if (fallbackPlant != null)
            {
                var speciesGene = GeneRegistry.Get<AgriPlant>(SpeciesGene.GENE_ID) as SpeciesGene;
                if (speciesGene != null)
                {
                    var allele = speciesGene.GetAllele(fallbackPlant);
                    if (allele != null)
                    {
                        genome.SetGenePair(speciesGene.GeneratePair(allele, allele));
                    }
                }
            }

            return genome;
        }
    }
}
