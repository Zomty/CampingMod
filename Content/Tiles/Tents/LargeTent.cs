﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.Localization;
using CampingMod.Common.Players;
using Terraria.GameContent;

namespace CampingMod.Content.Tiles.Tents
{
    public class LargeTent : ModTile
    {
        protected const int _FRAMEWIDTH = 6;
        protected const int _FRAMEHEIGHT = 3;
        int dropItem = 0;

        public override void SetStaticDefaults()
        {
            AddMapEntry(new Color(116, 117, 186), CreateMapEntryName());

            TileID.Sets.HasOutlines[Type] = true;
            TileID.Sets.CanBeSleptIn[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;

            CampingMod.Sets.TemporarySpawn.Add(Type);

            dropItem = ModContent.ItemType<Items.Tents.LargeTent>();

            TileID.Sets.CanBeSatOnForPlayers[Type] = true; // Facilitates calling ModifySittingTargetInfo for Players
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);

            //extra info
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = true;
            DustType = -1;
            AdjTiles = new int[] {
                    TileID.Beds, TileID.Chairs, TileID.Tables, TileID.Tables2,
                    TileID.WorkBenches, TileID.Bottles, TileID.CookingPots
                };

            CampTent.SetTentBaseTileObjectData(_FRAMEWIDTH, _FRAMEHEIGHT);
            //placement centre and offset on ground
            TileObjectData.newTile.Origin = new Point16(2, 2);

            // Add mirrored version from base, and commit object data
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
            TileObjectData.addAlternate(1);
            TileObjectData.addTile(Type);
        }

        public override void KillMultiTile(int tX, int tY, int pixelX, int pixelY)
        {
            Item.NewItem(new EntitySource_TileBreak(tX, tY), tX * 16, tY * 16, 16 * _FRAMEWIDTH, 16 * _FRAMEWIDTH, dropItem);
        }

        /// <summary>
        /// Allow smart select and drawing _Highlight.png
        /// </summary>
        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
        { return true; }

        public override bool RightClick(int tX, int tY)
        {
            Tile tile = Framing.GetTileSafely(tX, tY);
            int frameX = (tile.TileFrameX / 18) % _FRAMEWIDTH;
            int direction = (tile.TileFrameX >= 18 * _FRAMEWIDTH) ? -1 : 1;
            int logic = GetTileLogic(tX, tY);

            Player player = Main.LocalPlayer;

            if (logic == ItemID.WoodenChair) {
                int offsetX = (frameX == 1 || frameX == 4) ? -direction : 0;
                TileUtils.SetPlayerSitInChair(player, tX + offsetX, tY);
            }
            else {
                CampingModPlayer modPlayer = player.GetModPlayer<CampingModPlayer>();
                TileUtils.GetTentSpawnPosition(tX, tY, out int spawnX, out int spawnY, _FRAMEWIDTH, _FRAMEHEIGHT, 2, 1);
                TileUtils.ToggleTemporarySpawnPoint(modPlayer, spawnX, spawnY);
            }

            return true;
        }

        private static int GetTileLogic(int tX, int tY) {
            Tile tile = Main.tile[tX, tY];
            bool mirrored = (tile.TileFrameX >= 18 * _FRAMEWIDTH);
            int localTileX = tile.TileFrameX % (18 * _FRAMEWIDTH) / 18;
            int localTileY = tile.TileFrameY % (18 * _FRAMEHEIGHT) / 18;
            if (localTileY == 2) {
                if ((!mirrored && (localTileX == 0 || localTileX == 1))
                    ||
                    (mirrored && (localTileX == 4 || localTileX == 5))) {
                    return ItemID.WoodenChair; // A chair
                }
            }
            return -1;
        }


        public override void ModifySittingTargetInfo(int i, int j, ref TileRestingInfo info) {
            // It is very important to know that this is called on both players and NPCs, so do not use Main.LocalPlayer for example, use info.restingEntity
            Tile tile = Framing.GetTileSafely(i, j);
            bool mirrored = (tile.TileFrameX >= 18 * _FRAMEWIDTH);

            info.TargetDirection = mirrored ? 1 : -1;
            info.VisualOffset = new Vector2(-8, 2);

            info.AnchorTilePosition.X = i;
            info.AnchorTilePosition.Y = j;
        }

        public override void MouseOver(int tX, int tY)
        {
            int logic = GetTileLogic(tX, tY);
            if(logic == ItemID.WoodenChair) {
                TileUtils.ShowItemIcon(ItemID.WoodenChair);
            }
            else {
                TileUtils.ShowItemIcon(dropItem);
            }
        }
    }
}