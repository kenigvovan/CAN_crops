using cancrops.src.implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cancrops.src.utility
{
    // Registry for all configured plant types that can be used with the crop breeding system.
    // Plants are loaded from JSON configuration files at startup.
    public class AgriPlants
    {
        private Dictionary<string, AgriPlant> plants;

        public AgriPlants()
        {
            plants = new Dictionary<string, AgriPlant>();
        }
        
        // Check if a plant with the given id exists in the registry
        // "id" Full plant id in format "domain:name" (e.g., "game:carrot")
        public bool hasPlant(string id)
        {
            return plants.ContainsKey(id);
        }

        // Register a new plant in the system
        // "plant" The plant configuration to add
        // Returns true if successfully added, false if already exists
        public bool addPlant(AgriPlant plant)
        {
            return plants.TryAdd(plant.Domain + ":" + plant.Id, plant);
        }

        // Retrieve a plant configuration by its id
        // "id" Full plant id in format "domain:name" (e.g., "game:carrot")
        // Returns the plant configuration, or null if not found
        public AgriPlant getPlant(string id)
        {
            if (plants.TryGetValue(id, out AgriPlant val))
            {
                return val;
            }
            //TODO
            return null;
        }
    }
}
