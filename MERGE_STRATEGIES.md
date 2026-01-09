# Seed Merge Strategies

This document explains how the seed merge strategy configuration affects **crafting grid seed merging** in the CAN Crops mod.

## Important Note

The `seedMergeStrategy` configuration controls **crafting grid seed combining**, not the breeding system with selection sticks. When you craft multiple seeds together in the grid, this setting determines how their stats are merged.

**Breeding vs Crafting**:
- **Breeding** (selection sticks): Uses genetic recombination logic in `AgriCombineLogic.cs` with Mendelian inheritance
- **Crafting** (grid merging): Uses merge strategies in `CommonUtils.MergeGenomes()` as configured

## Overview

When combining multiple seeds in a crafting grid, their genetic stats must be merged. The `seedMergeStrategy` configuration option controls how this combination works.

## Available Strategies

### 1. "tolower" Strategy (Default)

**Behavior**: The result inherits the **lowest value** from all input seeds for each stat (both dominant and recessive alleles).

**Example**:
```
Seed 1: Gain=8, Growth=6, Strength=7
Seed 2: Gain=5, Growth=9, Strength=4

Result:
  Gain = min(8, 5) = 5
  Growth = min(6, 9) = 6
  Strength = min(7, 4) = 4
```

**Note**: This was fixed to correctly compare recessive values instead of incorrectly using dominant values in the recessive comparison.

**Use Case**: This strategy makes seed management more challenging. Players must be careful when combining seeds to avoid losing good stats.

**Pros**:
- Preserves worst-case values
- Prevents stat inflation from careless merging
- Encourages careful seed organization

**Cons**:
- Can lose good stats when combining seeds
- Less forgiving for inventory management
- May discourage seed consolidation

### 2. "mean" Strategy

**Behavior**: The child inherits the **average value** from all parent seeds for each stat, rounded using `MidpointRounding.AwayFromZero` (0.5 rounds away from zero).

**Example**:
```
Seed 1: Gain=8, Growth=6, Strength=7
Seed 2: Gain=5, Growth=9, Strength=4

Result:
  Gain = round((8 + 5) / 2) = 7 (6.5 rounds to 7)
  Growth = round((6 + 9) / 2) = 8 (7.5 rounds to 8)
  Strength = round((7 + 4) / 2) = 6 (5.5 rounds to 6)
```

**Note**: The rounding mode was fixed to use `AwayFromZero` instead of the default `ToEven` for more intuitive behavior.

**Use Case**: This strategy is more forgiving when combining seeds. Good for consolidating seed inventory.

**Pros**:
- Averages out stats fairly
- More forgiving for seed consolidation
- Can improve low-stat seeds by mixing with better ones

**Cons**:
- Can reduce high stats when mixed with low-stat seeds
- May lead to mediocre stat distribution
- Less control over final values

## How It's Configured

In your `cancrops.json` config file:

```json
{
  "seedMergeStrategy": "tolower",
  "seedMergeStrategies": ["tolower", "mean"]
}
```

- `seedMergeStrategy`: The currently active strategy
- `seedMergeStrategies`: List of valid options (for validation)

**To change the strategy**:
1. Open `%appdata%/VintagestoryData/ModConfig/cancrops.json` (Windows)
2. Or `~/.config/VintagestoryData/ModConfig/cancrops.json` (Linux)
3. Change `"seedMergeStrategy"` to `"mean"` or `"tolower"`
4. Restart the game

## Interaction with Breeding

**Important**: This merge strategy is for **crafting grid seed combining only**. It does NOT affect breeding with selection sticks.

**Crafting Grid Merging (uses merge strategy)**:
1. Place 2+ seeds in crafting grid
2. Apply configured merge strategy (tolower or mean)
3. Result seed has merged stats based on strategy
4. No mutations occur during crafting merge

**Breeding with Selection Sticks (different system)**:
1. Place selection sticks on farmland
2. Surround with parent crops
3. Uses genetic recombination in `AgriCombineLogic.cs`
4. Random allele selection from each parent
5. Mutations based on mutativity stat
6. Can result in different crop species (mutations)

## Which Strategy Should You Use?

### Choose "tolower" if you want:
- Challenging seed management
- Preserve only the best stats when combining seeds
- Prevent accidental stat dilution
- More careful seed organization needed

### Choose "mean" if you want:
- Average out stats when combining seeds
- More forgiving seed consolidation
- Gradual stat normalization across seed stock
- Focus on other game aspects while breeding on the side

## Technical Implementation

The merge strategy is implemented in `CommonUtils.MergeGenomes()` and triggered during crafting:

**Relevant files**:
- `Config.cs`: Defines the configuration option
- `CommonUtils.cs`: `MergeGenomesInnerMean()` implements both strategies
- `harmPatch.cs`: `Prefix_ItemPlantableSeed_OnCreatedByCrafting()` hooks into crafting

**Recent Fixes**:
- Fixed "mean" strategy to use `MidpointRounding.AwayFromZero` instead of default `ToEven`
- Fixed "tolower" strategy typo where dominant values were compared in recessive value selection

## Future Enhancements

Potential additional merge strategies for crafting:
- **"tohigher"**: Take the higher value (preserve best stats)
- **"random"**: Randomly pick from input seeds
- **"weighted"**: Weight by fertility stat
- **"first"**: Use first seed's stats (ignore others)

## Troubleshooting

**Q: I changed the strategy but my breeding results haven't changed?**
A: The merge strategy only affects **crafting grid seed merging**, not breeding with selection sticks. Breeding uses a separate genetic system.

**Q: Can I change the strategy mid-game?**
A: Yes, but it only affects future crafting. Already crafted seeds keep their stats.

**Q: Does this affect selection stick breeding?**
A: No, selection sticks use `AgriCombineLogic.cs` with genetic recombination, not merge strategies.

**Q: What happens with invalid strategy names?**
A: The merge will fail and return false, likely preventing the craft.

## See Also

- [README.md](README.md) - General mod documentation
- [ARCHITECTURE.md](ARCHITECTURE.md) - Technical implementation details
- Config.cs - Source code for configuration options
