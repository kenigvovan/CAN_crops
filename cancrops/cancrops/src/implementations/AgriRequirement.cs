using System.Collections.Generic;

namespace cancrops.src.implementations
{
    public class AgriRequirement
    {
        public int LightLevelType { get; set; }
        public int MinLight { get; set; }
        public int MaxLight { get; set; }
        public double LightToleranceFactor { get; set; }
        // Stage gating for spatial conditions: below this stage the Conditions block-checks
        // are skipped (light is still enforced). 0 = always enforce. Use to let a plant
        // sprout/grow anywhere up to this stage, then require its terrain block (e.g. ore)
        // for the final stages — visible feedback to the player instead of a silent block.
        public int RequirementFromStage { get; set; }
        public List<AgriBlockCondition> Conditions { get; set; }
    }
}
