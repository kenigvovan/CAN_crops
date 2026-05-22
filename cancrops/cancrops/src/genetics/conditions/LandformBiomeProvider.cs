using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.ServerMods;

namespace cancrops.src.genetics.conditions
{
    // Resolves a "biome name" for BiomeCondition by reading the world's LandformMap at the position.
    // Returns the landform variant's code path (e.g. "plains", "forest", "ocean") — JSON authors
    // match against these strings directly. Server-side only: requires NoiseLandforms.landforms
    // and a region size from the server API.
    public class LandformBiomeProvider : IBiomeProvider
    {
        public string GetBiomeName(IWorldAccessor world, BlockPos pos)
        {
            if (world == null || pos == null) return null;
            if (cancrops.sapi == null) return null; // client-side or pre-init: no answer

            var blockAccessor = world.BlockAccessor;
            IMapChunk mapChunk = blockAccessor.GetMapChunk(pos.X / 32, pos.Z / 32);
            if (mapChunk?.MapRegion?.LandformMap == null) return null;

            int regionSize = cancrops.sapi.WorldManager.RegionSize;
            if (regionSize <= 0) return null;

            float normX = (float)((double)pos.X / regionSize % 1.0);
            float normZ = (float)((double)pos.Z / regionSize % 1.0);

            int landformIndex = mapChunk.MapRegion.LandformMap.GetUnpaddedColorLerpedForNormalizedPos(normX, normZ);

            var landforms = NoiseLandforms.landforms;
            if (landforms?.LandFormsByIndex == null) return null;
            if (landformIndex < 0 || landformIndex >= landforms.LandFormsByIndex.Length) return null;

            return landforms.LandFormsByIndex[landformIndex]?.Code?.Path;
        }
    }
}
