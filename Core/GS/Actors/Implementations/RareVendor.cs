/*
 * Copyright (C) 2018-2019 DiIiS project
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System.Collections.Generic;
using NullD.Core.GS.Items;
using NullD.Core.GS.Map;
using NullD.Core.GS.Players;
using NullD.Net.GS.Message;
using NullD.Net.GS.Message.Definitions.Trade;
using NullD.Net.GS.Message.Definitions.World;
using NullD.Core.GS.Common;
using NullD.Core.GS.Common.Types.TagMap;
using NullD.Core.GS.Actors.Interactions;
using NullD.Core.GS.Actors;

namespace NullD.Core.GS.Actors.Implementations
{
    // TODO: this is just a test, do it properly for all vendors?
    [HandledSNO(
        182388, 230496, 230497, 230498, 230499, 230500, 161712,
        230501, 230502)]//

    public class RareVendor : InteractiveNPC
    {
        private InventoryGrid _vendorGrid;

        public RareVendor(World world, int snoId, TagMap tags)
            : base(world, snoId, tags)
        {
            this.Field7 = 1;
            this.Attributes[GameAttribute.MinimapActive] = true;

            _vendorGrid = new InventoryGrid(this, 1, 100, (int)EquipmentSlotId.Vendor);
            PopulateItems();
        }


        // TODO: Proper item loading from droplist?
        protected virtual List<Item> GetVendorItems()
        {
            var list = new List<Item>
            {
                ItemGenerator.GenerateWeaponToRareVendor(this),
                ItemGenerator.GenerateWeaponToRareVendor(this),
                ItemGenerator.GenerateWeaponToRareVendor(this),
                ItemGenerator.GenerateWeaponToRareVendor(this),
                ItemGenerator.GenerateWeaponToRareVendor(this),
                ItemGenerator.GenerateWeaponToRareVendor(this),

                ItemGenerator.GenerateArmorToRareVendor(this),
                ItemGenerator.GenerateArmorToRareVendor(this),
                ItemGenerator.GenerateArmorToRareVendor(this),
                ItemGenerator.GenerateArmorToRareVendor(this),
                ItemGenerator.GenerateArmorToRareVendor(this),
                ItemGenerator.GenerateArmorToRareVendor(this)
            };

            return list;
        }

        private void PopulateItems()
        {
            var items = GetVendorItems();

            if (items.Count > _vendorGrid.Columns)
            {
                _vendorGrid.ResizeGrid(1, items.Count);
            }

            foreach (var item in items)
            {
                _vendorGrid.AddItem(item);
            }
        }

        public override bool Reveal(Player player)
        {
            if (!base.Reveal(player))
                return false;

            _vendorGrid.Reveal(player);
            return true;
        }

        public override bool Unreveal(Player player)
        {
            if (!base.Unreveal(player))
                return false;

            _vendorGrid.Unreveal(player);
            return true;
        }

        public override void OnTargeted(Player player, TargetMessage message)
        {
            base.OnTargeted(player, message);
            player.InGameClient.SendMessage(new OpenTradeWindowMessage((int)this.DynamicID));
        }

        public virtual void OnRequestBuyItem(Player player, uint itemId)
        {
            // TODO: Check gold here
            Item item = _vendorGrid.GetItem(itemId);
            if (item == null)
                return;

            if (!player.Inventory.HasInventorySpace(item))
            {
                return;
            }


            player.Inventory.BuyItem(item);
            player.Inventory.RemoveGoldAmount(item.ItemDefinition.BaseGoldValue); // Remove the gold amount for buy a item [Necrosummon]
            //_vendorGrid.RemoveItem(item);
        }

        public virtual void OnRequestSellItem(Player player, uint itemId)
        {
            Item item = player.Inventory.GetItem(itemId);

            if (item == null)
                return;

            int SellGoldValue = item.ItemDefinition.BaseGoldValue / 25; // Cost of item to sell is splitted into 25 of her BaseGoldValue (Buy price) [Necrosummon]
            decimal.Floor(SellGoldValue);

            item.Attributes[Net.GS.Message.GameAttribute.Special_Inventory_Has_Sold] = true;
            item.Attributes[Net.GS.Message.GameAttribute.Item_Time_Sold] = 0;
            item.Attributes.BroadcastChangedIfRevealed();
            _vendorGrid.AddSelledItem(item);
            player.Inventory.SellItem(item);

            if (SellGoldValue <= 1) // if the operation have like a result less than 1, always vendor give you 1 gold for the item.
                player.Inventory.AddGoldAmount(1);
            else
                player.Inventory.AddGoldAmount(SellGoldValue);

            //RefreshInventoryToClient(player);
        }
        public void RefreshInventoryToClient(Player player)
        {
            var itemsToUpdate = new List<Item>();
            itemsToUpdate.AddRange(this._vendorGrid.Items.Values);

            foreach (var itm in itemsToUpdate)
            {
                //var player = (itm.Owner as GS.Players.Player);
                if (!itm.Reveal(player))
                {
                    player.InGameClient.SendMessage(itm.ACDInventoryPositionMessage);
                }

            }

        }
    }
}
