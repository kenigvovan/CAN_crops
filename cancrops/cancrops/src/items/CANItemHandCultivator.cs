using cancrops.src.blockenities;
using cancrops.src.blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace cancrops.src.items
{
    public class CANItemHandCultivator: Item
    {
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (blockSel == null)
            {
                return;
            }
            if (byEntity.Controls.ShiftKey && byEntity.Controls.CtrlKey)
            {
                base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
                return;
            }
            Block farmland = byEntity.World.BlockAccessor.GetBlock(blockSel.Position);
            if(farmland is CANBlockFarmland)
            {
                BlockEntity be = byEntity.World.BlockAccessor.GetBlockEntity(blockSel.Position);
                if (be is CANBlockEntityFarmland)
                {
                    ((CANBlockEntityFarmland)be).OnCultivating(slot, byEntity);
                    handling = EnumHandHandling.PreventDefault;
                }
            }           
        }
    }
}
