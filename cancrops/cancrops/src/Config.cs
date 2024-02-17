using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Datastructures;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace weightmod.src
{

    //
    //https://github.com/DArkHekRoMaNT
    //
    public class Config
    {

        // stats
        public int minGain = 1;
        public int maxGain = 10;
        public bool hiddenGain = false;
        public int minGrowth = 1;
        public int maxGrowth = 10;
        public bool hiddenGrowth = false;
        public int minStrength = 1;
        public int maxStrength = 10;
        public bool hiddenStrength = true;
        public int minResistance = 1;
        public int maxResistance = 10;
        public bool hiddenResistance = false;
        public int minFertility = 1;
        public int maxFertility = 10;
        public bool hiddenFertility = true;
        public int minMutativity = 1;
        public int maxMutativity = 10;
        public bool hiddenMutativity = true;
        public bool cloneMutations = true;

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
            { "resistance-heat",  "LightSalmon" }

        };
        public Dictionary<string, string> gene_color_int = new Dictionary<string, string>();
        
    }
}
