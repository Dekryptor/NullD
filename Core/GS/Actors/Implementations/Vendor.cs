/*
 * Copyright (C) 2011 - 2018 NullD project
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

namespace NullD.Core.GS.Actors.Implementations
{
    [HandledSNO(
         // Town_Inn
         109467, 180291,
         // Miner_InTown + variations
         177320, 178396, 178401, 178403, 229372, 229373, 229374, 229375, 229376,
         // Fence_InTown + variations
         177319, 178388, 178390, 178392, 229367, 229368, 229369, 229370, 229371,
         // Collector_InTown + variations
         107535, 178362, 178383, 178385, 229362, 229363, 229364, 229365, 229366,
         // Act 2
         180817, 180800, 180783, 180807, 180291, 230505,
         // Act 3
         //181467
         // Act 4
         182390, 230510, 230511, 230512, 230513, 230514, 230515, 230516,
         182389, 230503, 230504, 230505, 230506, 230507, 230508, 230509)]//230505 230512 

    public class Vendor : InteractiveNPC
    {
        private InventoryGrid _vendorGrid;
        public bool ItemCreated;

        public Vendor(World world, int snoId, TagMap tags)
            : base(world, snoId, tags)
        {
            //    Interactions.Add(new Unknown4Interaction());
            //    Interactions.Add(new InventoryInteraction());
            this.Field7 = 1;
            this.Attributes[GameAttribute.MinimapActive] = true;
            ItemCreated = false;
            _vendorGrid = new InventoryGrid(this, 1, 100, (int)EquipmentSlotId.Vendor);
            //PopulateItems();
        }


        // TODO: Proper item loading from droplist?
        protected virtual List<Item> GetVendorItems(Player plr)
        {
            List<Item> list;
            if (this.ActorSNO.Id == 109467)
            {
                list = AllPotions(plr);
            }
            else
            {
                list = new List<Item>
                {
                    ItemGenerator.GenerateWeaponToVendor(this, plr),
                    ItemGenerator.GenerateWeaponToVendor(this, plr),
                    ItemGenerator.GenerateWeaponToVendor(this, plr),
                    ItemGenerator.GenerateWeaponToVendor(this, plr),
                    ItemGenerator.GenerateWeaponToVendor(this, plr),
                    ItemGenerator.GenerateWeaponToVendor(this, plr),

                    ItemGenerator.GenerateArmorToVendor(this, plr),
                    ItemGenerator.GenerateArmorToVendor(this, plr),
                    ItemGenerator.GenerateArmorToVendor(this, plr),
                    ItemGenerator.GenerateArmorToVendor(this, plr),
                    ItemGenerator.GenerateArmorToVendor(this, plr),
                    ItemGenerator.GenerateArmorToVendor(this, plr),
                };
            }
            return list;
        }

        private void PopulateItems(Player plr)
        {
            var items = GetVendorItems(plr);

            if (items.Count > _vendorGrid.Columns)
            {
                _vendorGrid.ResizeGrid(1, items.Count);
            }

            foreach (var item in items)
            {
                _vendorGrid.AddItem(item);
            }
        }

        public List<Item> AllPotions(Player plr)
        {
            List<Item> list = new List<Item>
            {
                ItemGenerator.Cook(plr,"HealthPotionMinor"),
                ItemGenerator.Cook(plr,"HealthPotionLesser"),
                ItemGenerator.Cook(plr,"HealthPotion"),
                ItemGenerator.Cook(plr,"HealthPotionGreater"),
                ItemGenerator.Cook(plr,"HealthPotionMajor"),
                ItemGenerator.Cook(plr,"HealthPotionSuper"),
                ItemGenerator.Cook(plr,"HealthPotionHeroic"),
                ItemGenerator.Cook(plr,"HealthPotionResplendent"),
                ItemGenerator.Cook(plr,"HealthPotionRunic"),
                ItemGenerator.Cook(plr,"HealthPotionMythic"),
            };

            return list;
        }
        public override bool Reveal(Player player)
        {
            if (!base.Reveal(player))
                return false;
            if (ItemCreated == false)
            { PopulateItems(player); ItemCreated = true; }

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
            //_vendorGrid.AddSelledItem(item);
            //RefreshInventoryToClient(player);

        }
        public void RefreshInventoryToClient(Player player)
        {
            var itemsToUpdate = new List<Item>();
            itemsToUpdate.AddRange(this._vendorGrid.Items.Values);

            foreach (var itm in itemsToUpdate)
            {
                //var player = (itm.Owner as GS.Players.Player);
                try
                {
                    if (!itm.Reveal(player))
                    {
                        player.InGameClient.SendMessage(itm.ACDInventoryPositionMessage);
                    }
                }
                catch
                {

                }
            }

        }
    }
}
