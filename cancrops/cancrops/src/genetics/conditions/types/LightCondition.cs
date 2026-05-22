using Newtonsoft.Json.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace cancrops.src.genetics.conditions.types
{
    public class LightCondition : IMutationCondition
    {
        public const string TYPE_ID = "light";

        public int MinLight { get; }
        public int MaxLight { get; }
        public EnumLightLevelType LightLevelType { get; }
        public EnumConditionResult MetResult { get; }

        public LightCondition(int minLight, int maxLight, EnumLightLevelType lightLevelType, EnumConditionResult metResult)
        {
            MinLight = minLight;
            MaxLight = maxLight;
            LightLevelType = lightLevelType;
            MetResult = metResult;
        }

        public EnumConditionResult Check(IWorldAccessor world, BlockPos pos)
        {
            int light = world.BlockAccessor.GetLightLevel(pos, LightLevelType);
            bool met = light >= MinLight && light <= MaxLight;
            return met ? MetResult : EnumConditionResult.FORBID;
        }

        public static IMutationCondition FromJson(JObject json)
        {
            int min = (int?)json["min"] ?? 0;
            int max = (int?)json["max"] ?? 32;
            int typeInt = (int?)json["lightLevelType"] ?? (int)EnumLightLevelType.MaxLight;
            var result = ConditionJsonHelper.ParseResult(json["result"], EnumConditionResult.PASS);
            return new LightCondition(min, max, (EnumLightLevelType)typeInt, result);
        }
    }
}
