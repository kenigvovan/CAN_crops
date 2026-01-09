# CAN Crops Mod for Vintage Story

A Vintage Story mod (v1.21.6+) that adds a crop genetics and breeding system inspired by the Minecraft mod AgriCraft. This mod allows players to breed crops with better statistics through selective crossbreeding using selection sticks.

## Features

### Crop Statistics System

Seeds now have six genetic parameters that affect crop behavior:

- **Gain** - Amount of drops when the plant's block is broken
- **Growth** - Reduces time for the plant to reach the next growth stage
- **Strength** - Increases collected drop perish time (by +8% per stat level)
- **Resistance** - Changes border temperature for growing plants (by default ±0.4°C per stat level)
- **Fertility** - Inner parameter used for breeding logic (affects parent selection)
- **Mutativity** - Inner parameter used for stat mutation logic (affects genetic variation)

Each stat has a dominant and recessive allele value, following genetic inheritance patterns.

### Breeding System

To breed crops with better parameters, you need **Selection Sticks**:

1. Place selection sticks in a cross pattern on farmland
2. Plant compatible parent crops in adjacent farmland blocks
3. The selection sticks will combine genetics from surrounding parent plants
4. A new crop will appear on the cross sticks with inherited/mutated stats
5. The selection sticks are consumed in the process

The resulting crop can be:
- A genetic combination of parent crops (crossbreeding)
- A mutation into a new crop variety (if mutation conditions are met)

### Weed System

Crops can develop weeds that spread to nearby farmland if not managed:
- **Weed Stages**: None → Low → Medium → High
- Weeds spread to adjacent farmland based on crop resistance stat
- Higher resistance reduces weed spread probability
- Use the **Hand Cultivator** tool to remove weeds from crops
- Use **Anti-Weed** items for temporary weed protection

### Additional Features

- **Clipping**: Use shears on certain crops to harvest without destroying (if configured)
- **Custom Requirements**: Configure light, temperature, and block conditions for each crop
- **Mutation Recipes**: Define parent combinations that produce new crop varieties
- **Mod Compatibility**: Built-in support for XSkills, FarmlandDropsSoil, and PrimitiveSurvival mods

## Configuration

The mod creates a `cancrops.json` config file with the following options:

### Stat Ranges
```json
{
  "minGain": 0, "maxGain": 10,
  "minGrowth": 0, "maxGrowth": 10,
  "minStrength": 0, "maxStrength": 10,
  "minResistance": 0, "maxResistance": 10,
  "minFertility": 0, "maxFertility": 10,
  "minMutativity": 0, "maxMutativity": 10
}
```

### Visibility Settings
- `hiddenGain`: Hide gain stat from UI (default: false)
- `hiddenGrowth`: Hide growth stat from UI (default: false)
- `hiddenStrength`: Hide strength stat from UI (default: false)
- `hiddenResistance`: Hide resistance stat from UI (default: false)
- `hiddenFertility`: Hide fertility stat from UI (default: true)
- `hiddenMutativity`: Hide mutativity stat from UI (default: true)

### Merge Strategy
- `seedMergeStrategy`: How to merge parent stats ("tolower" or "mean")

### Weed Configuration
- `weedSpreadingActivated`: Enable/disable weed spreading (default: true)
- `weedAppearanceChance`: Base chance for weeds to appear (default: 0.15)
- `weedResistanceByStat`: Weed resistance per stat point (default: 0.05)
- `weedPropagationDepth`: How far weeds can spread (default: 4)

## Code Structure

### Core Classes

#### `cancrops.cs`
Main mod system class that:
- Registers blocks, items, and behaviors
- Loads configuration and plant/mutation recipes
- Applies Harmony patches to modify game behavior
- Handles client-server synchronization

#### Genetics System (`genetics/`)

- **`Genome.cs`**: Represents a complete set of 6 genes for a plant
- **`Gene.cs`**: Single genetic trait with dominant and recessive alleles
- **`Allele.cs`**: Individual allele value (0-10)
- **`AgriCombineLogic.cs`**: Handles crossbreeding between two parent genomes
- **`AgriCloneLogic.cs`**: Handles asexual reproduction/cloning
- **`AgriMutationHandler.cs`**: Manages mutation probability and stat changes
- **`ParentSelector.cs`**: Selects parent plants for breeding based on fertility

#### Block Entities (`BE/`)

- **`CANBECrop.cs`**: Attached to crop blocks, stores genome and handles interactions
- **`CANBECrossSticks.cs`**: Manages breeding logic on selection sticks
- **`WeedStage.cs`**: Enum for weed growth stages

#### Plant Configuration (`implementations/` & `templates/`)

- **`AgriPlant.cs`**: Runtime plant definition with requirements and drops
- **`AgriMutation.cs`**: Runtime mutation recipe (Parent1 + Parent2 → Child)
- **`JsonAgriPlant.cs`**: JSON template for plant configuration
- **`JsonAgriMutation.cs`**: JSON template for mutation recipes

#### Utility Classes (`utility/`)

- **`AgriPlants.cs`**: Registry for all configured plants
- **`AgriMutations.cs`**: Registry for all mutation recipes
- **`AgriPlantRequirementChecker.cs`**: Validates growing conditions
- **`CommonUtils.cs`**: Helper functions for colors, stats, and genetics

### Data Files

#### Plant Configuration (`resources/assets/cancrops/recipes/plants_jsons/`)
Defines plant behavior, requirements, and drops. Example:
```json
{
  "Enabled": true,
  "Domain": "game",
  "Id": "carrot",
  "GrowthMultiplier": 1.0,
  "AllowCloning": true,
  "Products": { ... },
  "Requirement": { ... }
}
```

#### Mutation Recipes (`resources/assets/cancrops/recipes/mutations_jsons/`)
Defines parent combinations for mutations. Example:
```json
{
  "Enabled": true,
  "Parent1": "game:carrot",
  "Parent2": "game:parsnip",
  "Child": "game:onion",
  "Chance": 0.05
}
```

## Commands

### `/cancrops setstat <statName> <statVal>`
Set a specific stat on the seed in your active hotbar slot.
- Requires admin privileges
- Stats: gain, growth, strength, resistance, fertility, mutativity
- Values: 0-10

## Building the Mod

1. Set `VINTAGE_STORY` environment variable to your Vintage Story installation path
2. Run `bash build.sh` (Linux/Mac) or `build.ps1` (Windows)
3. The mod will be built and packaged in `Releases/` folder

## Technical Notes

### Genetic Inheritance
- Each gene has a dominant and recessive allele
- Breeding randomly selects alleles from each parent
- Mutativity affects chance of stat mutation during breeding
- Fertility affects probability of being selected as a parent

### Merge Strategies
- **tolower**: Child inherits the lower value from parents
- **mean**: Child inherits the average value from parents

### Weed Mechanics
- Weeds appear randomly on crops with a configurable chance
- Higher crop resistance reduces weed appearance and spread
- Weeds progress through 3 stages: Low → Medium → High
- High-stage weeds can replace crops with wild grass blocks
- Weeds spread to adjacent crops recursively

## Credits

- Original concept inspired by AgriCraft (Minecraft mod)
- Developed by KenigVovan
- Forked and maintained by Andris0823

## License

See LICENSE file for details.
