using System.Collections;
using System.Collections.Generic;
using cancrops.src.genetics.abstractions;
using cancrops.src.genetics.genes;
using cancrops.src.implementations;
using Vintagestory.API.Datastructures;

namespace cancrops.src.genetics
{
    // Facade over MapGenome that preserves the existing legacy API:
    //   - Gain/Growth/Strength/Resistance/Fertility named accessors return Gene snapshots
    //   - Species exposes the dominant AgriPlant trait
    //   - GetGeneByName / SetGene / iteration / AsTreeAttribute / FromTreeAttribute keep working
    // Mutativity is intentionally removed — callers that referenced it will fail at compile time.
    public class Genome : IEnumerable<Gene>, IGenome
    {
        private MapGenome inner;

        // Legacy: hidden flags by stat name. Built from GeneRegistry on first access.
        private static Dictionary<string, bool> _genes;
        public static Dictionary<string, bool> genes
        {
            get
            {
                if (_genes == null || _genes.Count == 0)
                {
                    _genes = new Dictionary<string, bool>();
                    foreach (var g in GeneRegistry.AllGenes)
                    {
                        if (g is IntStatGene)
                        {
                            _genes[g.Id] = g.IsHidden;
                        }
                    }
                }
                return _genes;
            }
        }

        public static void InvalidateGenesCache() => _genes = null;

        public Gene Gain       => GetGeneSnapshot("gain");
        public Gene Growth     => GetGeneSnapshot("growth");
        public Gene Strength   => GetGeneSnapshot("strength");
        public Gene Resistance => GetGeneSnapshot("resistance");
        public Gene Fertility  => GetGeneSnapshot("fertility");

        // New: species accessor. Returns the dominant AgriPlant or null if unset.
        public AgriPlant Species
        {
            get
            {
                var gene = GeneRegistry.Get<AgriPlant>(SpeciesGene.GENE_ID);
                if (gene == null) return null;
                var pair = inner.GetGenePair<AgriPlant>(gene);
                return pair?.GetTrait();
            }
        }

        internal MapGenome Inner => inner;

        public Genome()
        {
            inner = new MapGenome();
            // Populate registered int genes with default alleles. Species is left unset —
            // callers (seed crafting, planting) inject it explicitly.
            foreach (var g in GeneRegistry.AllGenes)
            {
                if (g is IntStatGene)
                {
                    inner.SetGenePair(g.GenerateDefaultPair(null));
                }
            }
        }

        public Genome(List<Gene> legacyGenes) : this()
        {
            foreach (var g in legacyGenes)
            {
                SetGene(g.StatName, g);
            }
        }

        // Used by the migration path / cross-breed engine to take ownership of a built MapGenome.
        internal Genome(MapGenome inner)
        {
            this.inner = inner ?? new MapGenome();
        }

        public Gene GetGeneByName(string name) => GetGeneSnapshot(name);

        public bool SetGene(string name, Gene value)
        {
            var gene = GeneRegistry.Get<int>(name);
            if (gene == null || value == null) return false;
            var pair = gene.GeneratePair(
                gene.GetAllele(value.Dominant?.Value ?? gene.GetAllele(0).Trait),
                gene.GetAllele(value.Recessive?.Value ?? gene.GetAllele(0).Trait));
            inner.SetGenePair(pair);
            return true;
        }

        public void SetSpecies(AgriPlant plant)
        {
            var gene = GeneRegistry.Get<AgriPlant>(SpeciesGene.GENE_ID) as SpeciesGene;
            if (gene == null || plant == null) return;
            var allele = gene.GetAllele(plant);
            if (allele == null) return;
            inner.SetGenePair(gene.GeneratePair(allele, allele));
        }

        public Genome Clone()
        {
            return new Genome((MapGenome)inner.Clone());
        }

        IGenome IGenome.Clone() => Clone();

        public Gene Clone(Gene gene)
        {
            if (gene == null) return null;
            return GetGeneSnapshot(gene.StatName);
        }

        public ITreeAttribute AsTreeAttribute() => inner.AsTreeAttribute();

        // IGenome delegation
        public IGenePair GetGenePair(IGene gene) => inner.GetGenePair(gene);
        public IGenePair<T> GetGenePair<T>(IGene<T> gene) => inner.GetGenePair(gene);
        public void SetGenePair(IGenePair pair) => inner.SetGenePair(pair);
        public IEnumerable<IGenePair> AllPairs => inner.AllPairs;

        public static Genome FromTreeAttribute(ITreeAttribute tree, AgriPlant fallbackPlant = null)
        {
            if (tree == null) return null;
            var map = MapGenome.FromTreeAttribute(tree, fallbackPlant);
            return map == null ? null : new Genome(map);
        }

        public IEnumerator<Gene> GetEnumerator()
        {
            foreach (var name in genes.Keys)
            {
                var snap = GetGeneSnapshot(name);
                if (snap != null) yield return snap;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private Gene GetGeneSnapshot(string statName)
        {
            var gene = GeneRegistry.Get<int>(statName);
            if (gene == null) return null;
            var pair = inner.GetGenePair<int>(gene);
            if (pair == null) return null;
            return new Gene(statName, new Allele(pair.Dominant.Trait), new Allele(pair.Recessive.Trait))
            {
                Hidden = gene.IsHidden
            };
        }
    }
}
