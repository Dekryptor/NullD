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
using NullD.Common.Helpers.Math;
using NullD.Common.Logging;
using NullD.Common.Storage.AccountDataBase.Entities;
using NullD.Core.GS.Actors;
using NullD.Core.GS.Common.Types.Math;
using NullD.Core.GS.Objects;
using NullD.Core.GS.Players;
using NullD.Net.GS.Message.Definitions.World;
using NullD.Net.GS.Message.Definitions.Misc;
using NullD.Net.GS.Message.Fields;
using NullD.Net.GS.Message.Definitions.Effect;
using NullD.Net.GS.Message;
using NullD.Common.MPQ.FileFormats;
using Actor = NullD.Core.GS.Actors.Actor;
using World = NullD.Core.GS.Map.World;
using NullD.Core.GS.Common.Types.TagMap;
using NullD.Core.GS.Common.Types.SNO;

// TODO: This entire namespace belongs in GS. Bnet only needs a certain representation of items whereas nearly everything here is GS-specific

namespace NullD.Core.GS.Items
{
    /*
    public enum ItemType
    {
        Unknown, Helm, Gloves, Boots, Belt, Shoulders, Pants, Bracers, Shield, Quiver, Orb,
        Axe_1H, Axe_2H, CombatStaff_2H, Staff, Dagger, Mace_1H, Mace_2H, Sword_1H,
        Sword_2H, Crossbow, Bow, Spear, Polearm, Wand, Ring, FistWeapon_1H, ThrownWeapon, ThrowingAxe, ChestArmor,
        HealthPotion, Gold, HealthGlobe, Dye, Elixir, Charm, Scroll, SpellRune, Rune,
        Amethyst, Emarald, Ruby, Emerald, Topaz, Skull, Backpack, Potion, Amulet, Scepter, Rod, Journal,
        //CraftingReagent
        // Not working at the moment:
        // ThrownWeapon, ThrowingAxe - does not work because there are no snoId in Actors.txt. Do they actually drop in the D3 beta? /angerwin?
        // Diamond, Sapphire - I realised some days ago, that the Item type Diamond and Shappire (maybe not the only one) causes client crash and BAD GBID messages, although they actually have SNO IDs. /angerwin
    }
    */
    public class Item : Actor
    {
        public DBInventory DBInventory = null;
        public DBItemInstance DBItemInstance = null;

        private static readonly Logger Logger = LogManager.CreateLogger();
        public bool ItemHasChanges { get; private set; }//needed in Future, set this to true if Item affixes or item attributes have changed.


        public override ActorType ActorType { get { return ActorType.Item; } }

        public Actor Owner { get; set; } // Only set when the _actor_ has the item in its inventory. /fasbat

        public ItemTable ItemDefinition { get; private set; }
        public ItemTypeTable ItemType { get; private set; }

        public ItemRandomHelper RandomGenerator { get; private set; }
        public int ItemLevel { get; private set; }

        public ItemState CurrentState { get; set; }

        public int EquipmentSlot { get; private set; }
        public Vector2D InventoryLocation { get; private set; } // Column, row; NOTE: Call SetInventoryLocation() instead of setting fields on this

        public override int Quality
        {
            get
            {
                return Attributes[GameAttribute.Item_Quality_Level];
            }
            set
            {
                Attributes[GameAttribute.Item_Quality_Level] = value;
            }
        }

        public SNOHandle SnoFlippyActory
        {
            get
            {
                return ActorData.TagMap.ContainsKey(ActorKeys.Flippy) ? ActorData.TagMap[ActorKeys.Flippy] : null;
            }
        }

        public SNOHandle SnoFlippyParticle
        {
            get
            {
                return ActorData.TagMap.ContainsKey(ActorKeys.FlippyParticle) ? ActorData.TagMap[ActorKeys.FlippyParticle] : null;
            }
        }

        public override bool HasWorldLocation
        {
            get { return this.Owner == null; }
        }

        public override InventoryLocationMessageData InventoryLocationMessage
        {
            get
            {
                return new InventoryLocationMessageData
                {
                    OwnerID = (this.Owner != null) ? this.Owner.DynamicID : 0,
                    EquipmentSlot = this.EquipmentSlot,
                    InventoryLocation = this.InventoryLocation
                };
            }
        }

        public bool IsStackable()
        {
            return ItemDefinition.MaxStackAmount > 1;
        }

        public InvLoc InvLoc
        {
            get
            {
                return new InvLoc
                {
                    OwnerID = (this.Owner != null) ? this.Owner.DynamicID : 0,
                    EquipmentSlot = this.EquipmentSlot,
                    Row = this.InventoryLocation.Y,
                    Column = this.InventoryLocation.X
                };
            }
        }

        public Item(GS.Map.World world, ItemTable definition, IEnumerable<Affix> affixList, string serializedGameAttributeMap)
            : base(world, definition.SNOActor)
        {
            SetInitialValues(definition);
            this.Attributes.FillBySerialized(serializedGameAttributeMap);
            this.AffixList.Clear();
            this.AffixList.AddRange(affixList);

            // level requirement
            // Attributes[GameAttribute.Requirement, 38] = definition.RequiredLevel;
            /*
            Attributes[GameAttribute.Item_Quality_Level] = 1;
            if (Item.IsArmor(this.ItemType) || Item.IsWeapon(this.ItemType) || Item.IsOffhand(this.ItemType))
                Attributes[GameAttribute.Item_Quality_Level] = RandomHelper.Next(6);
            if (this.ItemType.Flags.HasFlag(ItemFlags.AtLeastMagical) && Attributes[GameAttribute.Item_Quality_Level] < 3)
                Attributes[GameAttribute.Item_Quality_Level] = 3;
            */
            //Attributes[GameAttribute.ItemStackQuantityLo] = 1;
            //Attributes[GameAttribute.Seed] = RandomHelper.Next(); //unchecked((int)2286800181);
            /*
            RandomGenerator = new ItemRandomHelper(Attributes[GameAttribute.Seed]);
            RandomGenerator.Next();
            if (Item.IsArmor(this.ItemType))
                RandomGenerator.Next(); // next value is used but unknown if armor
            RandomGenerator.ReinitSeed();*/
        }


        private void SetInitialValues(ItemTable definition)
        {
            this.ItemDefinition = definition;
            this.ItemLevel = definition.ItemLevel;
            this.GBHandle.Type = (int)GBHandleType.Gizmo;
            this.GBHandle.GBID = definition.Hash;
            this.ItemType = ItemGroup.FromHash(definition.ItemType1);
            this.EquipmentSlot = 0;
            this.InventoryLocation = new Vector2D { X = 0, Y = 0 };
            this.Scale = 1.0f;
            this.RotationW = 0.0f;
            this.RotationAxis.Set(0.0f, 0.0f, 1.0f);
            this.CurrentState = ItemState.Normal;
            this.Field2 = 0x00000000;
            this.Field7 = 0;
            this.NameSNOId = -1;      // I think it is ignored anyways - farmy
            this.Field10 = 0x00;
        }
        public Item(GS.Map.World world, ItemTable definition, bool Craft = false)
            : base(world, definition.SNOActor)
        {
            //Attributes[GameAttribute.Requirement,38] = (ulong)definition.RequiredLevel;
            SetInitialValues(definition);
            this.ItemHasChanges = true;//initial, this is set to true.
            // level requirement
            //Attributes[GameAttribute.Requirement, 316] = 10f;
            //Attributes[GameAttribute.Requirements_Ease_Percent, 38] = definition.RequiredLevel - 1;
            string[] parts = definition.Name.Split(new char[] { '_' });
            //Attributes[GameAttribute.Requirement,1] = definition.RequiredLevel;
            Attributes[GameAttribute.Item_Quality_Level] = 1;
            //Attributes[GameAttribute.Core_Attributes_From_Item_Bonus_Multiplier] = 1;

            this.Field2 = 0;

            this.Field11 = 0;
            Attributes[GameAttribute.IdentifyCost] = 1;
            //this.ItemType.Flags = this.ItemType.Flags | ItemFlags.Unknown | ItemFlags.Socketable;


            //this.Attributes[GameAttribute.Sockets] = 1;
            this.Attributes[GameAttribute.Sockets_Filled] = 0;
            if (Item.IsArmor(this.ItemType) || Item.IsWeapon(this.ItemType) || Item.IsOffhand(this.ItemType))
                Attributes[GameAttribute.Item_Quality_Level] = RandomHelper.Next(8);

            if (Attributes[GameAttribute.Item_Quality_Level] > 5 & Attributes[GameAttribute.Item_Quality_Level] < 9)
            {

                if (RandomHelper.Next(0, 100) < 85)
                {
                    Attributes[GameAttribute.Item_Quality_Level] -= 2;
                }
            }

            if (this.ItemType.Flags.HasFlag(ItemFlags.AtLeastMagical) && Attributes[GameAttribute.Item_Quality_Level] < 3)
                Attributes[GameAttribute.Item_Quality_Level] = 3;

            if (parts[0] == "Unique")
            {
                Logger.Debug("Геренация уникального предмета");
                Attributes[GameAttribute.Item_Quality_Level] = 9;

                if ((Item.IsArmor(this.ItemType)))
                {
                    Attributes[GameAttribute.Armor_Item] = definition.ItemLevel + RandomHelper.Next(0, 20);

                }
                if ((Item.IsWeapon(this.ItemType)))
                {
                    Attributes[GameAttribute.Attacks_Per_Second_Item] = 1.1f;
                    Attributes[GameAttribute.Damage_Weapon_Min, 0] = (definition.ItemLevel + definition.RequiredLevel + RandomHelper.Next(1, 4)) * 2;
                    Attributes[GameAttribute.Damage_Weapon_Delta, 0] += RandomHelper.Next(1, 3);
                    //scripted //Attributes[GameAttribute.Damage_Weapon_Max, 0] += Attributes[GameAttribute.Damage_Weapon_Min, 0] + Attributes[GameAttribute.Damage_Weapon_Delta, 0];

                }
                if (definition.SNOSet != -1)
                {
                    Attributes[GameAttribute.Item_Quality_Level] = 10;
                    if ((Item.IsArmor(this.ItemType)))
                    {
                        Attributes[GameAttribute.Armor_Item] = definition.ItemLevel * RandomHelper.Next(2, 4);
                    }
                    if ((Item.IsWeapon(this.ItemType)))
                    {
                        Attributes[GameAttribute.Damage_Weapon_Min] = (definition.ItemLevel + definition.RequiredLevel + RandomHelper.Next(2, 6)) * 2;
                        Attributes[GameAttribute.Damage_Weapon_Max] = (definition.ItemLevel + definition.RequiredLevel + RandomHelper.Next(7, 20)) * 2;
                    }
                }

            }
            else
            {
                if ((Item.IsWeapon(this.ItemType)))
                {
                    Attributes[GameAttribute.Damage_Weapon_Min, 0] += RandomHelper.Next(-2, 2);
                    Attributes[GameAttribute.Damage_Weapon_Delta, 0] += RandomHelper.Next(0, 2);
                }
                if ((Item.IsArmor(this.ItemType)))
                {
                    Attributes[GameAttribute.Armor_Item] += RandomHelper.Next(-1, 3);
                }
            }

            /*
             Inferior, 0
            Normal, 1
            Superior, 2
            Magic1, 3
            Magic2, 4
            Magic3, 5
            Rare4, 6
            Rare5, 7
            Rare6, 8
            Legendary, 9 
            Artifact, 10
            */
            Attributes[GameAttribute.ItemStackQuantityLo] = 1;
            Attributes[GameAttribute.Seed] = RandomHelper.Next(); //unchecked((int)2286800181);

            RandomGenerator = new ItemRandomHelper(Attributes[GameAttribute.Seed]);
            RandomGenerator.Next();
            if (Item.IsArmor(this.ItemType))
                RandomGenerator.Next(); // next value is used but unknown if armor
            RandomGenerator.ReinitSeed();

            ApplyWeaponSpecificOptions(definition);
            ApplyArmorSpecificOptions(definition);
            ApplyDurability(definition);
            ApplySkills(definition);
            ApplyAttributeSpecifier(definition);



            int affixNumber = 1;

            #region
            if (Craft == true)
            {
                if (definition.Name == "ChestArmor_002" ||
                    definition.Name == "Boots_002" ||
                    definition.Name == "Crossbow_001" ||
                    definition.Name == "Wand_001" ||
                    definition.Name == "Axe_1h_002" ||
                    definition.Name == "Pants_003" ||
                    definition.Name == "Bracers_002" ||
                    definition.Name == "Gloves_003" ||
                    definition.Name == "Shield_003" ||
                    definition.Name == "Sword_2h_002" ||
                    definition.Name == "Helm_003" ||
                    definition.Name == "Belt_003" ||
                    definition.Name == "Shoulders_002" ||
                    definition.Name == "CombatStaff_2H_002" ||
                    definition.Name == "Shoulders_003" ||
                    definition.Name == "Boots_004" ||
                    definition.Name == "MightyWeapon1H_003" ||
                    definition.Name == "Mace_2H_004" ||
                    definition.Name == "ChestArmor_005" ||
                    definition.Name == "Gloves_005" ||
                    definition.Name == "Bow_005" ||
                    definition.Name == "Wand_006" ||
                    definition.Name == "FistWeapon_1H_004" ||
                    definition.Name == "Boots_006" ||
                    definition.Name == "Axe_2H_005" ||
                    definition.Name == "Axe_1H_007" ||
                    definition.Name == "Wand_004"
                    )
                    Attributes[GameAttribute.Item_Quality_Level] = 4;
                if (definition.Name == "Quiver_004" || //Нужно добавить будет аффикс на +10% к скорости атаки.
                   definition.Name == "Quiver_005" ||
                   definition.Name == "Axe_1H_004" ||
                   definition.Name == "Handxbow_004" ||
                   definition.Name == "CeremonialDagger_1H_003" ||
                   definition.Name == "Sword_2H_004" ||
                   definition.Name == "Shield_006" ||
                   definition.Name == "Mace_1H_006" ||
                   definition.Name == "HandXbow_008" ||
                   definition.Name == "Bracers_006" ||
                   definition.Name == "CombatStaff_2H_004" ||
                   definition.Name == "ChestArmor_006" ||

                   definition.Name == "Mojo_004" ||//Нужно добавить аффикс на 3-5 дамага
                   definition.Name == "Dagger_006" ||
                   definition.Name == "Orb_003") //Нужно добавить будет аффикс на 5-6 дамага.
                    Attributes[GameAttribute.Item_Quality_Level] = 5;
                if (definition.Name == "Helm_004" ||
                    definition.Name == "Belt_006" ||
                    definition.Name == "Wand_007" ||
                    definition.Name == "Shoulders_006" ||
                    definition.Name == "Pants_006" ||
                    definition.Name == "Sword_1H_008")
                    Attributes[GameAttribute.Item_Quality_Level] = 6;
                if (definition.Name == "Staff_006" ||
                    definition.Name == "-1")
                    Attributes[GameAttribute.Item_Quality_Level] = 7;

            }
            #endregion

            if (Attributes[GameAttribute.Item_Quality_Level] >= 3)
                affixNumber = Attributes[GameAttribute.Item_Quality_Level] - 2;
            if (Attributes[GameAttribute.Item_Quality_Level] >= 3)
                affixNumber = Attributes[GameAttribute.Item_Quality_Level] - 2;
            AffixGenerator.Generate(this, affixNumber);
            //
            if (this.Quality > 5 & this.Quality < 9)
            {
                this.Field7 = 1;
                /*world..InGameClient.SendMessage(new GameSyncedDataMessage
                {
                    Field0 = GameData
                });
                RareItemNameMessage*/


                this.World.BroadcastIfRevealed(new RareItemNameMessage
                {
                    Field0 = (int)this.DynamicID,
                    Field1 = new RareItemName { snoAffixStringList = this.ItemDefinition.SNORareNamePrefixStringList }
                }, this);

            }

            this.Attributes.BroadcastChangedIfRevealed();

            //if (definition.Name == "Quiver_004")
            //    AffixList.Add(new Affix(2044719016));

        }




        private void ApplyWeaponSpecificOptions(ItemTable definition)
        {
            if (definition.WeaponDamageMin > 0)
            {
                Attributes[GameAttribute.Attacks_Per_Second_Item] += definition.AttacksPerSecond;
                //scripted //Attributes[GameAttribute.Attacks_Per_Second_Item_Subtotal] += definition.AttacksPerSecond;
                //scripted //Attributes[GameAttribute.Attacks_Per_Second_Item_Total] += definition.AttacksPerSecond;

                Attributes[GameAttribute.Damage_Weapon_Min, 0] += definition.WeaponDamageMin;
                //scripted //Attributes[GameAttribute.Damage_Weapon_Min_Total, 0] += definition.WeaponDamageMin;

                Attributes[GameAttribute.Damage_Weapon_Delta, 0] += definition.WeaponDamageDelta;
                //scripted //Attributes[GameAttribute.Damage_Weapon_Delta_SubTotal, 0] += definition.WeaponDamageDelta;
                //scripted //Attributes[GameAttribute.Damage_Weapon_Delta_Total, 0] += definition.WeaponDamageDelta;

                //scripted //Attributes[GameAttribute.Damage_Weapon_Max, 0] += Attributes[GameAttribute.Damage_Weapon_Min, 0] + Attributes[GameAttribute.Damage_Weapon_Delta, 0];
                //scripted //Attributes[GameAttribute.Damage_Weapon_Max_Total, 0] += Attributes[GameAttribute.Damage_Weapon_Min_Total, 0] + Attributes[GameAttribute.Damage_Weapon_Delta_Total, 0];

                //scripted //Attributes[GameAttribute.Damage_Weapon_Min_Total_All] = definition.WeaponDamageMin;
                //scripted //Attributes[GameAttribute.Damage_Weapon_Delta_Total_All] = definition.WeaponDamageDelta;
            }
        }

        private void ApplyArmorSpecificOptions(ItemTable definition)
        {
            if (definition.ArmorValue > 0)
            {
                Attributes[GameAttribute.Armor_Item] += definition.ArmorValue;
                //scripted //Attributes[GameAttribute.Armor_Item_SubTotal] += definition.ArmorValue;
                //scripted //Attributes[GameAttribute.Armor_Item_Total] += definition.ArmorValue;
            }
        }

        private void ApplyDurability(ItemTable definition)
        {
            if (definition.DurabilityMin > 0)
            {
                int durability = definition.DurabilityMin + RandomHelper.Next(definition.DurabilityDelta);
                Attributes[GameAttribute.Durability_Cur] = durability;
                Attributes[GameAttribute.Durability_Max] = durability;
            }
        }

        private void ApplySkills(ItemTable definition)
        {
            if (definition.SNOSkill0 != -1)
            {
                Attributes[GameAttribute.Skill, definition.SNOSkill0] = 1;
            }
            if (definition.SNOSkill1 != -1)
            {
                Attributes[GameAttribute.Skill, definition.SNOSkill1] = 1;
            }
            if (definition.SNOSkill2 != -1)
            {
                Attributes[GameAttribute.Skill, definition.SNOSkill2] = 1;
            }
            if (definition.SNOSkill3 != -1)
            {
                Attributes[GameAttribute.Skill, definition.SNOSkill3] = 1;
            }
        }

        private void ApplyAttributeSpecifier(ItemTable definition)
        {
            foreach (var effect in definition.Attribute)
            {
                float result;
                if (FormulaScript.Evaluate(effect.Formula.ToArray(), this.RandomGenerator, out result))
                {
                    //Logger.Debug("Randomized value for attribute " + GameAttribute.Attributes[effect.AttributeId].Name + " is " + result);

                    if (GameAttribute.Attributes[effect.AttributeId] is GameAttributeF)
                    {
                        var attr = GameAttribute.Attributes[effect.AttributeId] as GameAttributeF;
                        if (effect.SNOParam != -1)
                            Attributes[attr, effect.SNOParam] += result;
                        else
                            Attributes[attr] += result;
                    }
                    else if (GameAttribute.Attributes[effect.AttributeId] is GameAttributeI)
                    {
                        var attr = GameAttribute.Attributes[effect.AttributeId] as GameAttributeI;
                        if (effect.SNOParam != -1)
                            Attributes[attr, effect.SNOParam] += (int)result;
                        else
                            Attributes[attr] += (int)result;
                    }
                }
            }
        }

        // There are 2 VisualItemClasses... any way to use the builder to create a D3 Message?
        public VisualItem CreateVisualItem()
        {
            return new VisualItem()
            {
                GbId = this.GBHandle.GBID,
                Field1 = Attributes[GameAttribute.DyeType],
                Field2 = 0,
                Field3 = -1
            };
        }

        //TODO: Move to proper D3.Hero.Visual item classes
        public D3.Hero.VisualItem GetVisualItem()
        {
            var visualItem = D3.Hero.VisualItem.CreateBuilder()
                .SetGbid(this.GBHandle.GBID)
                .SetDyeType(Attributes[GameAttribute.DyeType])
                .SetEffectLevel(0)
                .SetItemEffectType(-1)
                .Build();
            return visualItem;
        }

        #region Is*
        public static bool IsHealthGlobe(ItemTypeTable itemType)
        {
            return ItemGroup.IsSubType(itemType, "HealthGlyph");
        }

        public static bool IsGold(ItemTypeTable itemType)
        {
            return ItemGroup.IsSubType(itemType, "Gold");
        }

        public static bool IsPotion(ItemTypeTable itemType)
        {
            return ItemGroup.IsSubType(itemType, "Potion");
        }

        public static bool IsAccessory(ItemTypeTable itemType)
        {
            return ItemGroup.IsSubType(itemType, "Jewelry");
        }

        public static bool IsRuneOrJewel(ItemTypeTable itemType)
        {
            return ItemGroup.IsSubType(itemType, "Gem") || ItemGroup.IsSubType(itemType, "SpellRune");
        }

        public static bool IsJournalOrScroll(ItemTypeTable itemType)
        {
            return ItemGroup.IsSubType(itemType, "Scroll") || ItemGroup.IsSubType(itemType, "Book");
        }

        public static bool IsDye(ItemTypeTable itemType)
        {
            return ItemGroup.IsSubType(itemType, "Dye");
        }

        public static bool IsWeapon(ItemTypeTable itemType)
        {
            return ItemGroup.IsSubType(itemType, "Weapon");
        }

        public static bool IsArmor(ItemTypeTable itemType)
        {
            return ItemGroup.IsSubType(itemType, "Armor");
        }

        public static bool IsOffhand(ItemTypeTable itemType)
        {
            return ItemGroup.IsSubType(itemType, "Offhand");
        }

        public static bool Is2H(ItemTypeTable itemType)
        {
            return ItemGroup.Is2H(itemType);
        }
        #endregion

        public void SetInventoryLocation(int equipmentSlot, int column, int row)
        {
            this.EquipmentSlot = equipmentSlot;
            this.InventoryLocation.X = column;
            this.InventoryLocation.Y = row;
            if (this.Owner is GS.Players.Player)
            {
                var player = (this.Owner as GS.Players.Player);
                if (!this.Reveal(player))
                {
                    player.InGameClient.SendMessage(this.ACDInventoryPositionMessage);
                }
            }
        }

        public void SetNewWorld(World world)
        {
            if (this.World == world)
                return;

            this.World = world;
        }

        public void Drop(Player owner, Vector3D position)
        {
            this.Owner = owner;
            this.EnterWorld(position);
        }

        public override void OnTargeted(Player player, TargetMessage message)
        {
            //Logger.Trace("OnTargeted");
            player.Inventory.PickUp(this);
        }

        public virtual void OnRequestUse(Player player, Item target, int actionId, WorldPlace worldPlace)
        {
            throw new System.NotImplementedException();
        }

        public override bool Reveal(Player player)
        {
            if (this.CurrentState == ItemState.PickingUp && HasWorldLocation)
                return false;

            if (!base.Reveal(player))
                return false;

            var affixGbis = new int[AffixList.Count];
            for (int i = 0; i < AffixList.Count; i++)
            {
                affixGbis[i] = AffixList[i].AffixGbid;
            }

            player.InGameClient.SendMessage(new AffixMessage()
            {
                ActorID = DynamicID,
                Field1 = 0x00000001,
                aAffixGBIDs = affixGbis,
            });

            player.InGameClient.SendMessage(new AffixMessage()
            {
                ActorID = DynamicID,
                Field1 = 0x00000002,
                aAffixGBIDs = new int[0],
            });

            return true;
        }

        public override bool Unreveal(Player player)
        {
            if (CurrentState == ItemState.PickingUp && player == Owner)
            {
                return false;
            }
            return base.Unreveal(player);
        }
    }

    public enum ItemState
    {
        Normal,
        PickingUp,
        Dropping
    }
}
