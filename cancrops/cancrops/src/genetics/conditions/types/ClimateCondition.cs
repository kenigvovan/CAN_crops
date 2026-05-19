using Newtonsoft.Json.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace cancrops.src.genetics.conditions.types
{
    public class ClimateCondition : IMutationCondition
    {
        public const string TYPE_ID = "climate";

        public float MinTemp { get; }
        public float MaxTemp { get; }
        public float MinRain { get; }
        public float MaxRain { get; }
        public EnumConditionResult MetResult { get; }

        public ClimateCondition(float minTemp, float maxTemp, float minRain, float maxRain, EnumConditionResult metResult)
        {
            MinTemp = minTemp;
            MaxTemp = maxTemp;
            MinRain = minRain;
            MaxRain = maxRain;
            MetResult = metResult;
        }

        public EnumConditionResult Check(IWorldAccessor world, BlockPos pos)
        {
            var conds = world.BlockAccessor.GetClimateAt(pos, EnumGetClimateMode.NowValues);
            if (conds == null) return EnumConditionResult.PASS;
            bool met = conds.Temperature >= MinTemp && conds.Temperature <= MaxTemp
                    && conds.Rainfall   >= MinRain && conds.Rainfall   <= MaxRain;
            return met ? MetResult : EnumConditionResult.FORBID;
        }

        public static IMutationCondition FromJson(JObject json)
        {
            float minT = (float?)json["minTemp"] ?? -100f;
            float maxT = (float?)json["maxTemp"] ?? 100f;
            float minR = (float?)json["minRain"] ?? 0f;
            float maxR = (float?)json["maxRain"] ?? 1f;
            var result = ConditionJsonHelper.ParseResult(json["result"], EnumConditionResult.PASS);
            return new ClimateCondition(minT, maxT, minR, maxR, result);
        }
    }
}
