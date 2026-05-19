using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace cancrops.src.genetics.conditions
{
    public delegate IMutationCondition ConditionFactory(JObject json);

    public static class ConditionRegistry
    {
        private static readonly Dictionary<string, ConditionFactory> factories = new Dictionary<string, ConditionFactory>();

        public static void Register(string typeId, ConditionFactory factory)
        {
            if (string.IsNullOrEmpty(typeId)) throw new ArgumentException("typeId is required");
            factories[typeId] = factory;
        }

        public static bool Contains(string typeId) => factories.ContainsKey(typeId);

        // Builds a condition from JSON. Returns null if the "type" field is missing or unknown.
        public static IMutationCondition Build(JObject json)
        {
            if (json == null) return null;
            string typeId = (string)json["type"];
            if (typeId == null) return null;
            return factories.TryGetValue(typeId, out var factory) ? factory(json) : null;
        }

        public static void Clear() => factories.Clear();
    }
}
