using System.Collections.Generic;

namespace cancrops.src
{
    public class Config
    {

        // stats
        public int minGain = 0;
        public int maxGain = 10;
        public bool hiddenGain = false;
        public int minGrowth = 0;
        public int maxGrowth = 10;
        public bool hiddenGrowth = false;
        public int minStrength = 0;
        public int maxStrength = 10;
        public bool hiddenStrength = false;
        public int minResistance = 0;
        public int maxResistance = 10;
        public bool hiddenResistance = false;
        public int minFertility = 0;
        public int maxFertility = 10;
        public bool hiddenFertility = true;
        public bool cloneMutations = true;

        // int stat mutator (AgriCraft-style, replaces Mutativity-driven mutation in Phase 3+)
        // Active strategy: "default" / "none" / "hill_climb" / "gaussian" / "reset_on_low"
        public string activeStatMutator = "default";
        public double statMutationChance = 0.25;
        public int statMutationStep = 1;
        // hill_climb: probability that the mutation goes up (rest goes down). 0.5 = symmetric, 1.0 = always up.
        public double statMutationUpBias = 0.7;
        // gaussian: standard deviation of the normal distribution applied to the value.
        public double statMutationSigma = 1.0;
        // reset_on_low: when value == 0, chance to randomise upward to [1, max].
        public double statResetOnZeroChance = 0.10;

        // How a int-stat gene's pair expresses a trait from its (dominant, recessive) values: MIN/MEAN/MAX
        public string statTraitLogic = "MAX";

        // Per-tick chance that a cross-sticks block with 2+ valid parents will attempt a combine.
        // Effective rate is also throttled by the cross-sticks tick interval (~20s).
        public double crossBreedTickChance = 0.5;

        public float coldResistanceByStat = 0.4f;
        public float heatResistanceByStat = 0.4f;
        public float strengthFreshHoursPercentBonus = 0.8f;

        //tags
        public string genome_tag = "genome";
        public Dictionary<string, string> gene_color = new Dictionary<string, string>
        {
            { "gain", "peru" },
            { "growth", "green" },
            { "resistance", "steelblue" },
            { "strength", "Tomato" },
            { "resistance-cold",  "SlateBlue" },
            { "resistance-heat",  "LightSalmon" },
            { "fertility", "SeaGreen"},
            { "species", "MediumOrchid"},
        };
        public Dictionary<string, string> gene_color_int = new Dictionary<string, string>();
        public int weedMinimumSpreadStage = 1;
        public double weedResistanceByStat = 0.05;
        public double weedChanceForCrossSticks = 0.15;
        public double weedStageBonusAdditionForSpawnChance = 0.05;
        public int weedPropagationDepth = 4;
        public Dictionary<int, double> weedSpreadChancePerStage = new Dictionary<int, double>{
            { 0, 0.1 },
            { 1, 0.3 },
            { 2, 0.5 }
        };
        public bool weedSpreadingActivated = true;
        public double weedAppearanceChance = 0.15;
        public Dictionary<string, bool> hidden_genes = new Dictionary<string, bool>
        {
            {"gain", false},
            {"growth", false},
            {"strength", false},
            {"resistance", false},
            {"fertility", true}
        };
        public string seedMergeStrategy = "tolower";
        public HashSet<string> seedMergeStrategies = ["tolower", "mean"];
        public Dictionary<string, double> weedBlockCodes = new() {
			{ "tallgrass-veryshort-free", 0.95 },
			{  "tallgrass-short-free",  0.85 },
			{  "tallgrass-mediumshort-free",  0.78 },
			{  "tallgrass-medium-free",  0.67 },
			{  "flower-horsetail-free",  0.2 },
		};
        public double minHoursBetweenWeedStages = 3;
        public Dictionary<string, int> weedProtectionTicks = new() {
           {"canantiweed-temporal", 1000 }
        };
    }
}
