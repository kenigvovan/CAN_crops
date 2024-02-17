using cancrops.src.templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cancrops.src.utility
{
    public class AgriPlants
    {
        private Dictionary<string, AgriPlant> plants;

        public AgriPlants()
        {
            plants = new Dictionary<string, AgriPlant>();
        }
        public bool hasPlant(string id)
        {
            return plants.ContainsKey(id);
        }

        public bool addPlant(AgriPlant plant)
        {
            return plants.TryAdd(plant.Id, plant);
        }

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
