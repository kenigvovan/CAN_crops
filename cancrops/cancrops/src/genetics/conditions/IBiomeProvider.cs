using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace cancrops.src.genetics.conditions
{
    // Pluggable biome lookup. cancrops doesn't ship its own biome detection — provider can be
    // registered by an external mod (e.g. CANAntiqueAtlas) to wire real biome ids/names.
    // Default implementation returns null → BiomeCondition treats no allowlist as always-met,
    // and allowlist as never-met.
    public interface IBiomeProvider
    {
        // Returns a stable biome name/id at the given position, or null if unknown.
        string GetBiomeName(IWorldAccessor world, BlockPos pos);
    }

    public static class BiomeProviderRegistry
    {
        public static IBiomeProvider Provider { get; private set; } = new NullBiomeProvider();

        public static void Set(IBiomeProvider provider)
        {
            Provider = provider ?? new NullBiomeProvider();
        }
    }

    internal class NullBiomeProvider : IBiomeProvider
    {
        public string GetBiomeName(IWorldAccessor world, BlockPos pos) => null;
    }
}
