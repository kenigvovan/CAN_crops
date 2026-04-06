# CAN Crops - Architecture Documentation

This document explains the internal architecture and logic of the CAN Crops mod for developers.

## Overview

The CAN Crops mod implements a genetic breeding system for crops in Vintage Story, inspired by AgriCraft from Minecraft. The system uses Mendelian genetics principles with dominant/recessive alleles and mutation mechanics.

## Core Concepts

### Genetics System

The genetics system follows these principles:

1. **Genome**: A complete set of 6 genes representing all inheritable traits
2. **Gene**: A single trait with two alleles (dominant and recessive)
3. **Allele**: A variant of a gene with an integer value (0-10)
4. **Phenotype**: The expressed trait (always uses the dominant allele value)
5. **Genotype**: The underlying genetic makeup (both alleles)

### Breeding Mechanics

When two parent plants are crossbred using selection sticks:

1. **Parent Selection**: Parents are chosen based on their fertility stat
2. **Allele Inheritance**: For each gene, one allele is randomly selected from each parent
3. **Mutation**: Based on mutativity stat, alleles may mutate (±1)
4. **Recombination**: The selected alleles form the offspring's genotype
5. **Plant Mutation**: Sometimes the offspring can be a different crop species

### Statistical Effects

Each stat affects gameplay differently:

- **Gain (0-10)**: Multiplies harvest yield by `(1 + gain * 0.1)`. Gain 5 = 150% yield.
- **Growth (0-10)**: Reduces growth time. Higher values = faster maturation.
- **Strength (0-10)**: Increases perish time by 8% per level. Strength 5 = 40% longer freshness.
- **Resistance (0-10)**: 
  - Temperature: Expands viable temperature range by ±0.4°C per level
  - Weeds: Reduces weed appearance/spread by 5% per level
- **Fertility (0-10)**: Internal stat. Higher = more likely to be selected as parent.
- **Mutativity (0-10)**: Internal stat. Affects mutation probability during breeding.

## Code Organization

### Directory Structure

```
cancrops/cancrops/src/
├── BE/                          # Block Entities
│   ├── CANBECrop.cs            # Crop block entity - stores genome
│   ├── CANBECrossSticks.cs     # Breeding station block entity
│   └── WeedStage.cs            # Weed progression enum
├── blocks/
│   └── CANBlockSelectionSticks.cs  # Cross sticks block behavior
├── commands/
│   └── SetStatsCommands.cs     # Admin commands for setting stats
├── compat/                      # Mod compatibility patches
│   ├── farmlanddropssoil/
│   ├── primitivesurvival/
│   └── xskills/
├── cropBehaviors/
│   └── AgriPlantCropBehavior.cs    # Crop behavior hooks
├── genetics/                    # Core genetics system
│   ├── Allele.cs               # Single allele (gene variant)
│   ├── Gene.cs                 # Gene with two alleles
│   ├── Genome.cs               # Complete genetic makeup
│   ├── AgriCombineLogic.cs     # Crossbreeding logic
│   ├── AgriCloneLogic.cs       # Asexual reproduction
│   ├── AgriMutationHandler.cs  # Mutation probability & handling
│   └── ParentSelector.cs       # Parent selection by fertility
├── implementations/             # Runtime data structures
│   ├── AgriPlant.cs            # Plant definition with requirements
│   ├── AgriMutation.cs         # Mutation recipe (parent combo)
│   ├── AgriProductList.cs      # Drop table
│   ├── AgriRequirement.cs      # Growing conditions
│   └── AgriBlockCondition.cs   # Block proximity requirements
├── items/
│   ├── CANItemHandCultivator.cs    # Weed removal tool
│   └── CANItemAntiWeed.cs      # Weed prevention item
├── templates/                   # JSON deserialization classes
│   ├── JsonAgriPlant.cs
│   ├── JsonAgriMutation.cs
│   ├── JsonAgriProduct.cs
│   ├── JsonAgriProductList.cs
│   ├── JsonAgriRequirement.cs
│   └── JsonAgriBlockCondition.cs
├── utility/                     # Helper classes
│   ├── AgriPlants.cs           # Plant registry
│   ├── AgriMutations.cs        # Mutation recipe registry
│   ├── AgriPlantRequirementChecker.cs  # Validates growing conditions
│   ├── CommonUtils.cs          # Color/stat helpers
│   ├── ConfigUpdateValuesPacket.cs     # Network sync
│   ├── EnumCropSticksVariant.cs
│   ├── IAgriRegistrable.cs
│   └── MutationParentsTupleComparer.cs
├── Config.cs                    # Mod configuration
├── cancrops.cs                 # Main mod system class
└── harmPatch.cs                # Harmony patches for game hooks
```

## Key Systems

### 1. Plant Registry System

**Files**: `AgriPlants.cs`, `AgriPlant.cs`, `JsonAgriPlant.cs`

Plants are loaded from JSON files at mod startup:
1. `cancrops.InitPlants()` scans `recipes/plants_jsons/`
2. Each JSON is deserialized to `JsonAgriPlant`
3. Converted to `AgriPlant` (resolves item references)
4. Registered in `AgriPlants` dictionary by `domain:id`

**Plant Configuration Format**:
```json
{
  "Enabled": true,
  "Domain": "game",
  "Id": "carrot",
  "GrowthMultiplier": 1.0,
  "AllowCloning": true,
  "AllowSourceStage": 7,
  "Products": {
    "Products": [
      {
        "CollectibleCode": "game:carrot",
        "ItemClass": "Item",
        "Avg": 2.0,
        "Var": 1.0,
        "LastDrop": false
      }
    ]
  },
  "Requirement": {
    "MinLight": 9,
    "MaxLight": 19,
    "LightLevelType": "MaxLight"
  }
}
```

### 2. Mutation System

**Files**: `AgriMutations.cs`, `AgriMutation.cs`, `JsonAgriMutation.cs`, `AgriMutationHandler.cs`

Mutations define special breeding outcomes:
1. When breeding, `AgriMutationHandler.handleCrossBreedTick()` checks for mutations
2. Looks up mutation recipes matching the parent pair
3. Uses `randomMutate()` to check probability
4. If successful, offspring is the mutation child instead of parent species

**Mutation Format**:
```json
{
  "Enabled": true,
  "Parent1": "game:carrot",
  "Parent2": "game:parsnip",
  "Child": "game:onion",
  "Chance": 0.05
}
```

### 3. Breeding Process

**Main Flow** (`CANBECrossSticks.cs` and `AgriMutationHandler.cs`):

```
1. Player places selection sticks on farmland
2. Plants parent crops in adjacent blocks (N/S/E/W)
3. Selection sticks tick (time passes)
4. On breeding tick:
   a. Scan for neighboring crops (CANBECrop entities)
   b. Select parents using ParentSelector (fertility-based)
   c. Check for species mutation (AgriMutationHandler)
   d. If mutation: create child of mutation species
   e. If no mutation: crossbreed using AgriCombineLogic
   f. Set genome on new crop
   g. Remove selection sticks
```

**AgriCombineLogic.combine()** details:
```csharp
For each gene in genome:
  1. Pick random allele from parent1 gene (dominant or recessive)
  2. Pick random allele from parent2 gene (dominant or recessive)
  3. For each picked allele:
     - Check if mutation occurs (based on parent's mutativity)
     - If mutate: ±1 to allele value (weighted by mutativity)
  4. Create child gene with picked alleles as dominant/recessive
  5. Add to child genome
```

### 4. Weed System

**Files**: `CANBECrop.cs`, `CANBECrossSticks.cs`, `WeedStage.cs`

Weeds can appear and spread on crops:

**Weed Lifecycle**:
1. Random chance on crop tick to spawn weed (configurable)
2. Weeds progress: NONE → LOW → MEDIUM → HIGH
3. HIGH stage can replace crop with wild grass
4. Weeds spread recursively to adjacent crops

**Resistance Effect**:
- Base weed chance: 15% (configurable)
- Resistance reduces chance: `baseChance - (resistance * 0.05)`
- Example: Resistance 5 = 15% - 25% = no weeds possible

**Weed Removal**:
- Hand Cultivator: Manually removes weeds (damages tool)
- Anti-Weed items: Temporary protection (timed)

### 5. Harmony Patches

**File**: `harmPatch.cs`

The mod modifies vanilla game behavior using Harmony patches:

**Key Patches**:
- `GetPlacedBlockInfo`: Shows seed stats in tooltip
- `GetDrops`: Modifies crop drops based on gain stat
- `GetHoursForNextStage`: Adjusts growth time based on growth stat
- `OnBlockInteractStart`: Handles shears for clipping
- `Update` (transpilers): Temperature tolerance based on resistance stat

### 6. Client-Server Synchronization

**Files**: `ConfigUpdateValuesPacket.cs`, `cancrops.cs`

Config values are synced to clients:
1. Server loads config on startup
2. When player joins: `SendUpdatedConfigValues()` sends packet
3. Client receives and updates local config copy
4. Ensures consistent visibility of hidden stats

## Data Flow Examples

### Example 1: Harvesting a Crop

```
1. Player breaks crop block (BlockCrop.OnBlockBroken)
2. Harmony patch intercepts BlockEntityFarmland.GetDrops
3. Check if CANBECrop entity exists
4. If yes: read gain stat from genome
5. Multiply drop quantity: base * (1 + gain * 0.1)
6. Apply strength stat to perish time: base * (1 + strength * 0.08)
7. Return modified drops
```

### Example 2: Temperature Check

```
1. BlockEntityFarmland.Update checks temperature
2. Transpiler patch injects resistance stat check
3. Reads resistance from CANBECrop genome
4. Adjusts cold threshold: base - (resistance * 0.4°C)
5. Adjusts heat threshold: base + (resistance * 0.4°C)
6. Crop survives in wider temperature range
```

### Example 3: Creating a Mutation

```
1. Selection sticks tick with 4 adjacent crops
2. Parent 1: Carrot (fertility 8)
3. Parent 2: Parsnip (fertility 6)
4. Parent 3: Turnip (fertility 4)
5. Parent 4: Onion (fertility 2)
6. ParentSelector sorts by fertility: Carrot, Parsnip selected
7. Check mutations: Carrot+Parsnip → Onion (5% chance)
8. Roll: 0.034 < 0.05 → SUCCESS
9. Create onion crop with combined genetics
10. Onion inherits mix of carrot+parsnip stats
```

## Configuration System

The `Config.cs` defines all tunable parameters:

**Stat Ranges**: Min/max values for each genetic stat
**Visibility**: Which stats show in UI vs hidden
**Merge Strategy**: How parent stats combine ("tolower" or "mean")
**Weed Settings**: Appearance, spread, and resistance mechanics
**Resistance Effects**: Temperature and perish time multipliers
**Gene Colors**: UI color coding for different stats

## Performance Considerations

1. **Lazy Initialization**: `Genome.genes` uses lazy initialization to avoid static initialization order issues
2. **Dictionary Lookups**: Plants and mutations stored in dictionaries for O(1) lookup
3. **Selective Updates**: Only crops with weeds/breeding tick regularly
4. **Client-Side Rendering**: Weed meshes cached per block entity

## Common Issues and Solutions

### Issue: Static Initialization Order
**Problem**: `Genome.genes` dictionary tried to access `cancrops.config` before initialization.
**Solution**: Changed to lazy initialization with null-coalescing defaults.

### Issue: Parent Genome Bug
**Problem**: `AgriCombineLogic` used `parents.Item1` twice instead of Item1 and Item2.
**Solution**: Fixed line 13 to use `parents.Item2` for second parent.

### Issue: Hidden Stats
**Problem**: Important stats like Strength were hidden by default.
**Solution**: Changed default visibility in Config.cs.

## Extending the Mod

### Adding a New Stat

1. Add to `Config.cs`: min, max, hidden, and gene color
2. Add property to `Genome.cs`
3. Update `Genome` constructor and serialization
4. Add to `genes` dictionary
5. Implement effect in relevant patches or behaviors

### Adding a New Crop

1. Create JSON in `recipes/plants_jsons/`
2. Define products, requirements, cloning behavior
3. Mod will auto-register on next load

### Adding a Mutation Recipe

1. Create JSON in `recipes/mutations_jsons/`
2. Specify parent1, parent2, child, and chance
3. Mod will auto-register on next load

## Testing

Since this is a game mod, testing requires:
1. Installing Vintage Story
2. Building the mod
3. Loading in-game
4. Manual testing of breeding mechanics

**Test Checklist**:
- [ ] Seeds show stats in tooltip
- [ ] Selection sticks can be crafted
- [ ] Breeding produces offspring with inherited stats
- [ ] Mutations occur at configured rates
- [ ] Weeds appear and spread correctly
- [ ] Gain stat affects harvest yield
- [ ] Growth stat affects crop speed
- [ ] Strength stat affects perish time
- [ ] Resistance stat affects temperature tolerance
- [ ] Hand cultivator removes weeds
- [ ] Shears can clip crops (if configured)

## Credits

Based on AgriCraft concepts by InfinityRaider (Minecraft mod).
Implemented for Vintage Story by KenigVovan.
Refactored and documented by Andris0823.
