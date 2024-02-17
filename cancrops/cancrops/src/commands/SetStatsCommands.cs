using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace cancrops.src.commands
{
    public static class SetStatsCommands
    {
        public static TextCommandResult SetSeedStatCommand(TextCommandCallingArgs args) //ItemStack itemStack, string statName, int newVal)
        {
            TextCommandResult tcr = new TextCommandResult();
            tcr.Status = EnumCommandStatus.Success;
            IServerPlayer player = args.Caller.Player as IServerPlayer;

            ItemStack activeItemStack = player.InventoryManager.ActiveHotbarSlot.Itemstack ?? null;
            if(activeItemStack == null)
            {
                return tcr;
            }
            ITreeAttribute tree = activeItemStack.Attributes.GetTreeAttribute(cancrops.config.genome_tag);
            if(tree == null)
            {
                return tcr;
            }

            ITreeAttribute searchedGeneTree = tree.GetTreeAttribute((string)args.Parsers[0].GetValue());
            if(searchedGeneTree == null) 
            {
                return tcr;
            }

            searchedGeneTree.SetInt("D", (int)args.Parsers[1].GetValue());
            return tcr;
        }
    }
}
