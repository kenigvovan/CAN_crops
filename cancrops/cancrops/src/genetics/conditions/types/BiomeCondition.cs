using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace cancrops.src.genetics.conditions.types
{
    public class BiomeCondition : IMutationCondition
    {
        public const string TYPE_ID = "biome";

        public HashSet<string> AllowedBiomes { get; }
        public EnumConditionResult MetResult { get; }

        public BiomeCondition(IEnumerable<string> biomes, EnumConditionResult metResult)
        {
            AllowedBiomes = new HashSet<string>(biomes);
            MetResult = metResult;
        }

        public EnumConditionResult Check(IWorldAccessor world, BlockPos pos)
        {
            // Empty allowlist means any biome → met
            if (AllowedBiomes.Count == 0) return MetResult;

            string biome = BiomeProviderRegistry.Provider.GetBiomeName(world, pos);
            // Provider unknown → treat as met to avoid false-FORBID when atlas mod is absent.
            // Authors who require strict biome gating should ensure a real provider is registered.
            if (biome == null) return MetResult;

            bool met = AllowedBiomes.Contains(biome);
            return met ? MetResult : EnumConditionResult.FORBID;
        }

        public static IMutationCondition FromJson(JObject json)
        {
            var biomes = new List<string>();
            var arr = json["biomes"] as JArray;
            if (arr != null)
            {
                foreach (var t in arr) biomes.Add((string)t);
            }
            var result = ConditionJsonHelper.ParseResult(json["result"], EnumConditionResult.PASS);
            return new BiomeCondition(biomes, result);
        }
    }
}
