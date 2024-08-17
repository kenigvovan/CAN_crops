using cancrops.src.blockenities;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using XLib.XLeveling;
using XSkills;

namespace cancrops.src.compat.xskills
{
    [HarmonyPatch(typeof(BlockWateringCan))]
    public class WatteringCanPatch
    {
        public static bool Prepare(MethodBase original)
        {
            XSkills.XSkills instance = XSkills.XSkills.Instance;
            if (instance == null)
            {
                return false;
            }
            Skill skill;
            instance.Skills.TryGetValue("farming", out skill);
            Farming farming = skill as Farming;
            if (farming == null || !farming.Enabled)
            {
                return false;
            }
            if (original == null)
            {
                return true;
            }
            if (original.Name == "OnHeldInteractStep")
            {
                return farming[farming.ExtensiveFarmingId].Enabled;
            }
            return farming.Enabled;
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnHeldInteractStep")]
        public static void Postfix(bool __result, float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel)
        {
            if (!__result)
            {
                return;
            }
            IWorldAccessor world = byEntity.World;
            Block block = world.BlockAccessor.GetBlock(blockSel.Position);
            float @float = slot.Itemstack.TempAttributes.GetFloat("secondsUsed", 0f);
            XLeveling xleveling = XLeveling.Instance(byEntity.Api);
            Farming farming = ((xleveling != null) ? xleveling.GetSkill("farming", false) : null) as Farming;
            if (farming == null)
            {
                return;
            }
            PlayerSkillSet behavior = byEntity.GetBehavior<PlayerSkillSet>();
            PlayerAbility playerAbility;
            if (behavior == null)
            {
                playerAbility = null;
            }
            else
            {
                PlayerSkill playerSkill = behavior[farming.Id];
                playerAbility = ((playerSkill != null) ? playerSkill[farming.ExtensiveFarmingId] : null);
            }
            int num = playerAbility.Value(0, 0);
            if (num == 0)
            {
                return;
            }
            int num2 = blockSel.Position.X;
            int y = blockSel.Position.Y;
            int num3 = blockSel.Position.Z;
            int num4 = 0;
            int num5 = 0;
            if (num % 2 == 0)
            {
                if ((double)num2 - byEntity.Pos.X >= 0.0)
                {
                    num4 = 1;
                }
                if ((double)num3 - byEntity.Pos.Z >= 0.0)
                {
                    num5 = 1;
                }
            }
            num2 = num2 - num / 2 + num4;
            num3 = num3 - num / 2 + num5;
            for (int i = num2; i < num2 + num; i++)
            {
                for (int j = num3; j < num3 + num; j++)
                {
                    if (i != blockSel.Position.X || j != blockSel.Position.Z)
                    {
                        BlockPos blockPos = new BlockPos(i, y, j);
                        if (block.CollisionBoxes == null || block.CollisionBoxes.Length == 0)
                        {
                            block = world.BlockAccessor.GetBlock(blockSel.Position, 2);
                            if ((block.CollisionBoxes == null || block.CollisionBoxes.Length == 0) && !block.IsLiquid())
                            {
                                blockPos = blockPos.DownCopy(1);
                            }
                        }
                        CANBlockEntityFarmland blockEntityFarmland = world.BlockAccessor.GetBlockEntity(blockPos) as CANBlockEntityFarmland;
                        if (blockEntityFarmland != null)
                        {
                            blockEntityFarmland.WaterFarmland(secondsUsed - @float, true);
                        }
                    }
                }
            }
        }
    }
}
