using System;
using System.Collections.Generic;

namespace cancrops.src.genetics.abstractions
{
    public static class GeneRegistry
    {
        private static readonly Dictionary<string, IGene> genesById = new Dictionary<string, IGene>();
        private static readonly List<IGene> genesList = new List<IGene>();

        public static IReadOnlyList<IGene> AllGenes => genesList;

        public static void Register(IGene gene)
        {
            if (gene == null) throw new ArgumentNullException(nameof(gene));
            if (genesById.ContainsKey(gene.Id))
                throw new InvalidOperationException($"Gene with id '{gene.Id}' is already registered");
            genesById[gene.Id] = gene;
            genesList.Add(gene);
        }

        public static IGene Get(string id)
        {
            return genesById.TryGetValue(id, out var gene) ? gene : null;
        }

        public static IGene<T> Get<T>(string id)
        {
            return Get(id) as IGene<T>;
        }

        public static bool Contains(string id) => genesById.ContainsKey(id);

        public static void Clear()
        {
            genesById.Clear();
            genesList.Clear();
        }
    }
}
