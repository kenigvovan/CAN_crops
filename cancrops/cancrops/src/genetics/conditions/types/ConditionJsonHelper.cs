using Newtonsoft.Json.Linq;

namespace cancrops.src.genetics.conditions.types
{
    internal static class ConditionJsonHelper
    {
        public static EnumConditionResult ParseResult(JToken token, EnumConditionResult fallback)
        {
            string raw = (string)token;
            if (string.IsNullOrEmpty(raw)) return fallback;
            switch (raw.ToUpperInvariant())
            {
                case "FORBID": return EnumConditionResult.FORBID;
                case "PASS":   return EnumConditionResult.PASS;
                case "FORCE":  return EnumConditionResult.FORCE;
                default:       return fallback;
            }
        }
    }
}
