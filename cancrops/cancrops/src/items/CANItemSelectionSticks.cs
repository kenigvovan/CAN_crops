using cancrops.src.blockenities;
using cancrops.src.blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace cancrops.src.items
{
    public class CANItemSelectionSticks : Item//, ITexPositionSource, IContainedMeshSource
    {
        private ITextureAtlasAPI targetAtlas;
        private ICoreClientAPI capi;
        private Dictionary<string, AssetLocation> tmpTextures = new Dictionary<string, AssetLocation>();
        public TextureAtlasPosition this[string textureCode]
        {
            get
            {
                return this.getOrCreateTexPos(this.tmpTextures[textureCode]);
            }
        }

        public Size2i AtlasSize
        {
            get
            {
                return this.targetAtlas.Size;
            }
        }
        protected TextureAtlasPosition getOrCreateTexPos(AssetLocation texturePath)
        {
            TextureAtlasPosition texpos = this.targetAtlas[texturePath];
            if (texpos == null)
            {
                IAsset texAsset = this.capi.Assets.TryGet(texturePath.Clone().WithPathPrefixOnce("textures/").WithPathAppendixOnce(".png"), true);
                if (texAsset != null)
                {
                    int num;
                    this.targetAtlas.GetOrInsertTexture(texturePath, out num, out texpos, () => texAsset.ToBitmap(this.capi), 0.005f);
                }
                else
                {
                    this.capi.World.Logger.Warning("For render in shield {0}, require texture {1}, but no such texture found.", new object[]
                    {
                        this.Code,
                        texturePath
                    });
                }
            }
            return texpos;
        }
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            this.capi = (api as ICoreClientAPI);
        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if(blockSel == null)
            {
                base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
            }
            Block selectedBlock = api.World.BlockAccessor.GetBlock(blockSel.Position);
            if (selectedBlock is CANBlockFarmland || selectedBlock is BlockCrop)
            {
                BlockEntity cbef = api.World.BlockAccessor.GetBlockEntity(selectedBlock is CANBlockFarmland 
                                                                                                                ? blockSel.Position
                                                                                                                : blockSel.Position.DownCopy());
                if (cbef == null)
                {
                    return;
                }
                if (!(cbef is CANBlockEntityFarmland))
                {
                    return;
                }
                
                if((cbef as CANBlockEntityFarmland).GetCropSticksVariant() < utility.EnumCropSticksVariant.DOUBLE)
                {
                    if (this.api.Side == EnumAppSide.Client)
                    {
                        handling = EnumHandHandling.PreventDefault;
                        return;
                    }
                    if((cbef as CANBlockEntityFarmland).TryPlaceSelectionSticks())
                    {
                        IPlayer player = null;
                        if (byEntity is EntityPlayer)
                        {
                            player = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
                        }
                        if (player == null || player.WorldData?.CurrentGameMode != EnumGameMode.Creative)
                        {
                            slot.TakeOut(1);
                            slot.MarkDirty();
                        }
                    }
                }
                return;
            }
        }
        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            base.OnHeldInteractStop(secondsUsed, slot, byEntity, blockSel, entitySel);
        }
    }
}
