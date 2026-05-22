using System.Collections.Generic;

namespace cancrops.src.templates
{
    public class JsonAgriRequirement
    {
        public int LightLevelType { get; set; }
        public int MinLight { get; set; }
        public int MaxLight { get; set; }
        public double LightToleranceFactor { get; set; }
        public int RequirementFromStage { get; set; }
        public List<JsonAgriBlockCondition> Conditions { get; set; }
    }
}
