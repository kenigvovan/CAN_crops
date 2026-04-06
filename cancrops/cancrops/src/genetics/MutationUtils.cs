using System;

namespace cancrops.src.genetics
{
    public static class MutationUtils
    {
        // Mutates an allele value based on mutativity stat.
        // Chance: mutativity/max (e.g. 5/10 = 50%)
        // Direction: biased toward positive by mutativity
        // Magnitude: usually ±1, at high mutativity (>=8) 10% chance of ±2
        public static int MutateAllele(int value, int mutativity, Random random)
        {
            int max = cancrops.config.maxMutativity;

            // Roll for mutation chance: higher mutativity = higher chance
            if (random.Next(max) >= mutativity)
            {
                return value;
            }

            // Determine magnitude: ±1, or ±2 at high mutativity
            int magnitude = 1;
            if (mutativity >= 8 && random.Next(10) == 0)
            {
                magnitude = 2;
            }

            // Direction biased by mutativity: higher = more likely positive
            // mutativity 1: ~55% up / 45% down
            // mutativity 5: ~75% up / 25% down
            // mutativity 10: 100% up
            int delta = random.Next(max * 2) < (max + mutativity) ? magnitude : -magnitude;

            int newValue = value + delta;

            // Clamp to valid range
            return Math.Clamp(newValue, 1, max);
        }
    }
}
