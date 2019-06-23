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

using System;
using System.Collections.Generic;
using System.Linq;
using NullD.Common.MPQ;
using NullD.Common.MPQ.FileFormats;
using NullD.Common.Storage;
using NullD.Common.Storage.AccountDataBase.Entities;
using NullD.Core.LogNet.Accounts;
using NullD.Core.LogNet.Helpers;
using NullD.Core.LogNet.Objects;
using NullD.Core.GS.Players;
using NHibernate.Linq;
using NullD.Common.Helpers.Math;
using NullD.Core.GS.Actors;
using NullD.Net.GS.Message;

namespace NullD.Core.LogNet.Toons
{
    public class Toon : PersistentRPCObject
    {
        public DBToon DBToon { get; private set; }
        public IntPresenceField HeroClassField
        {
            get
            {
                var val = new IntPresenceField(FieldKeyHelper.Program.D3, FieldKeyHelper.OriginatingClass.Hero, 1, 0, this.ClassID);
                return val;
            }
        }
        public IntPresenceField HeroFlagsField
        {
            get
            {
                var val = new IntPresenceField(FieldKeyHelper.Program.D3, FieldKeyHelper.OriginatingClass.Hero, 4, 0, (int)this.DBToon.Flags);
                return val;
            }
        }
        public IntPresenceField HeroLevelField
        {
            get
            {
                var val = new IntPresenceField(FieldKeyHelper.Program.D3, FieldKeyHelper.OriginatingClass.Hero, 2, 0, this.DBToon.Level);
                return val;
            }
        }
        public bool Hardcore
        {
            get { return this.DBToon.Hardcore; }
            set { this.DBToon.Hardcore = value; }
        }

        public bool Dead
        {
            get { return this.DBToon.Dead; }
            set { this.DBToon.Dead = value; }
        }

        public bool StoneOfPortal
        {
            get { return this.DBToon.StoneOfPortal; }
            set { this.DBToon.StoneOfPortal = value; }
        }

        public string LoreCollected
        {
            get { return this.DBToon.DBProgressToon.LoreCollected; }
            set { this.DBToon.DBProgressToon.LoreCollected = value; }
        }
        public StringPresenceField HeroNameField
        { get { return new StringPresenceField(FieldKeyHelper.Program.D3, FieldKeyHelper.OriginatingClass.Hero, 5, 0, this.DBToon.Name); } }


        /// <summary>
        /// Level Of Hirelings.
        /// </summary>
        public int LevelOfTemplar { get { return this.DBToon.DBHirelingsOfToon.Level_Templar; } set { this.DBToon.DBHirelingsOfToon.Level_Templar = value; } }
        public int LevelOfScoundrel { get { return this.DBToon.DBHirelingsOfToon.Level_Scoundrel; } set { this.DBToon.DBHirelingsOfToon.Level_Scoundrel = value; } }
        public int LevelOfEnchantress { get { return this.DBToon.DBHirelingsOfToon.Level_Enchantress; } set { this.DBToon.DBHirelingsOfToon.Level_Enchantress = value; } }

        /// <summary>
        /// Level Of Hirelings.
        /// </summary>
        public int ExperienceOfTemplar { get { return this.DBToon.DBHirelingsOfToon.Experience_Templar; } set { this.DBToon.DBHirelingsOfToon.Experience_Templar = value; } }
        public int ExperienceOfScoundrel { get { return this.DBToon.DBHirelingsOfToon.Experience_Scoundrel; } set { this.DBToon.DBHirelingsOfToon.Experience_Scoundrel = value; } }
        public int ExperienceOfEnchantress { get { return this.DBToon.DBHirelingsOfToon.Experience_Enchantress; } set { this.DBToon.DBHirelingsOfToon.Experience_Enchantress = value; } }

        /// <summary>
        /// Skill of Scoundrel.
        /// </summary>
        public int Templar_Skill1 { get { return this.DBToon.DBHirelingsOfToon.Templar_Skill1; } set { this.DBToon.DBHirelingsOfToon.Templar_Skill1 = value; } }
        public int Templar_Skill2 { get { return this.DBToon.DBHirelingsOfToon.Templar_Skill2; } set { this.DBToon.DBHirelingsOfToon.Templar_Skill2 = value; } }
        public int Templar_Skill3 { get { return this.DBToon.DBHirelingsOfToon.Templar_Skill3; } set { this.DBToon.DBHirelingsOfToon.Templar_Skill3 = value; } }
        public int Templar_Skill4 { get { return this.DBToon.DBHirelingsOfToon.Templar_Skill4; } set { this.DBToon.DBHirelingsOfToon.Templar_Skill4 = value; } }

        /// <summary>
        /// Skill of Scoundrel.
        /// </summary>
        public int Scoundrel_Skill1 { get { return this.DBToon.DBHirelingsOfToon.Scoundrel_Skill1; } set { this.DBToon.DBHirelingsOfToon.Scoundrel_Skill1 = value; } }
        public int Scoundrel_Skill2 { get { return this.DBToon.DBHirelingsOfToon.Scoundrel_Skill2; } set { this.DBToon.DBHirelingsOfToon.Scoundrel_Skill2 = value; } }
        public int Scoundrel_Skill3 { get { return this.DBToon.DBHirelingsOfToon.Scoundrel_Skill3; } set { this.DBToon.DBHirelingsOfToon.Scoundrel_Skill3 = value; } }
        public int Scoundrel_Skill4 { get { return this.DBToon.DBHirelingsOfToon.Scoundrel_Skill4; } set { this.DBToon.DBHirelingsOfToon.Scoundrel_Skill4 = value; } }

        /// <summary>
        /// Skill of Enchantress.
        /// </summary>
        public int Enchantress_Skill1 { get { return this.DBToon.DBHirelingsOfToon.Enchantress_Skill1; } set { this.DBToon.DBHirelingsOfToon.Enchantress_Skill1 = value; } }
        public int Enchantress_Skill2 { get { return this.DBToon.DBHirelingsOfToon.Enchantress_Skill2; } set { this.DBToon.DBHirelingsOfToon.Enchantress_Skill2 = value; } }
        public int Enchantress_Skill3 { get { return this.DBToon.DBHirelingsOfToon.Enchantress_Skill3; } set { this.DBToon.DBHirelingsOfToon.Enchantress_Skill3 = value; } }
        public int Enchantress_Skill4 { get { return this.DBToon.DBHirelingsOfToon.Enchantress_Skill4; } set { this.DBToon.DBHirelingsOfToon.Enchantress_Skill4 = value; } }

        /// <summary>
        /// Total time played for toon.
        /// </summary>
        public int StatusOfWings { get { return this.DBToon.DBProgressToon.StatusOfWings; } set { this.DBToon.DBProgressToon.StatusOfWings = value; } }

        public uint WayPointStatus { get { return this.DBToon.DBProgressToon.WaypointStatus; } set { this.DBToon.DBProgressToon.WaypointStatus = value; } }
        public uint WayPointStatus2 { get { return this.DBToon.DBProgressToon.WaypointStatus2; } set { this.DBToon.DBProgressToon.WaypointStatus2 = value; } }
        public uint WayPointStatus3 { get { return this.DBToon.DBProgressToon.WaypointStatus3; } set { this.DBToon.DBProgressToon.WaypointStatus3 = value; } }
        public uint WayPointStatus4 { get { return this.DBToon.DBProgressToon.WaypointStatus4; } set { this.DBToon.DBProgressToon.WaypointStatus4 = value; } }


        public ByteStringPresenceField<D3.Hero.VisualEquipment> HeroVisualEquipmentField = new ByteStringPresenceField<D3.Hero.VisualEquipment>(FieldKeyHelper.Program.D3, FieldKeyHelper.OriginatingClass.Hero, 3, 0);

        public int MaximumAct { get { return this.DBToon.DBProgressToon.MaximumAct; } set { this.DBToon.DBProgressToon.MaximumAct = value; } }
        public int MaximumQuest { get { return this.DBToon.DBProgressToon.MaximumQuest; } set { this.DBToon.DBProgressToon.MaximumQuest = value; } }
        public int ActiveQuest { get { return this.DBToon.DBProgressToon.ActiveQuest; } set { this.DBToon.DBProgressToon.ActiveQuest = value; } }

        public int ActiveAct {
            get { return this.DBToon.DBProgressToon.ActiveAct; }
            set { this.DBToon.DBProgressToon.ActiveAct = value; }
        }

        public int StepIDofQuest { get { return this.DBToon.DBProgressToon.StepIDofQuest; } set { this.DBToon.DBProgressToon.StepIDofQuest = value; } }

        public int StepOfQuest { get { return this.DBToon.DBProgressToon.StepOfQuest; } set { this.DBToon.DBProgressToon.StepOfQuest = value; } }

        public int Side_ActiveQuest { get { return this.DBToon.DBProgressToon.Side_ActiveQuest; } set { this.DBToon.DBProgressToon.Side_ActiveQuest = value; } }
        public int Side_StepIDofQuest { get { return this.DBToon.DBProgressToon.Side_StepIDofQuest; } set { this.DBToon.DBProgressToon.Side_StepIDofQuest = value; } }
        public int Side_StepOfQuest { get { return this.DBToon.DBProgressToon.Side_StepOfQuest; } set { this.DBToon.DBProgressToon.Side_StepOfQuest = value; } }

        public byte AltLevel
        {
            get
            {
                return DBToon.AltLevel;
            }
            private set
            {
                this.DBToon.AltLevel = value;
            }
        }


        public IntPresenceField HighestUnlockedAct = new IntPresenceField(FieldKeyHelper.Program.D3, FieldKeyHelper.OriginatingClass.Hero, 6, 0, 0);

        public IntPresenceField HighestUnlockedDifficulty = new IntPresenceField(FieldKeyHelper.Program.D3, FieldKeyHelper.OriginatingClass.Hero, 7, 0, 0);

        /// <summary>
        /// D3 EntityID encoded id.
        /// </summary>
        public D3.OnlineService.EntityId D3EntityID { get; private set; }

        /// <summary>
        /// True if toon has been recently deleted;
        /// </summary>
        public bool Deleted
        {
            get { return this.DBToon.Deleted; }
            set { this.DBToon.Deleted = value; }
        }



        /// <summary>
        /// Toon handle struct.
        /// </summary>
        public ToonHandleHelper ToonHandle { get; private set; }

        /// <summary>
        /// Toon's name.
        /// </summary>
        public string Name
        {
            get
            {
                return this.DBToon.Name;
            }
            private set
            {
                this.DBToon.Name = value;
                this.HeroNameField.Value = value;
            }
        }

        /*
        /// <summary>
        /// Toon's hash-code.
        /// </summary>
        public int HashCode { get; set; }
        */
        /// <summary>
        /// Toon's owner account.
        /// </summary>
        public GameAccount GameAccount { get { return GameAccountManager.GetGameAccountByDBGameAccount(this.DBToon.DBGameAccount); } set { this.DBToon.DBGameAccount = value.DBGameAccount; } }

        /// <summary>
        /// Toon's class.
        /// </summary>
        public ToonClass Class
        {
            get
            {
                return DBToon.Class;
            }
            private set
            {
                DBToon.Class = value;
                /*
                switch (DBToon.Class)
                {
                    case ToonClass.Barbarian:
                        this.HeroClassField.Value = 0x4FB91EE2;
                        break;
                    case ToonClass.DemonHunter:
                        this.HeroClassField.Value = unchecked((int)0xC88B9649);
                        break;
                    case ToonClass.Monk:
                        this.HeroClassField.Value = 0x3DAC15;
                        break;
                    case ToonClass.WitchDoctor:
                        this.HeroClassField.Value = 0x343C22A;
                        break;
                    case ToonClass.Wizard:
                        this.HeroClassField.Value = 0x1D4681B1;
                        break;
                    default:
                        this.HeroClassField.Value = 0x0;
                        break;
                }*/
            }
        }

        /// <summary>
        /// Toon's flags.
        /// </summary>
        public ToonFlags Flags
        {
            get
            {
                return this.DBToon.Flags;
            }
            private set
            {
                this.DBToon.Flags = value | ToonFlags.AllUnknowns;
                //this.HeroFlagsField.Value = (int)(value | ToonFlags.AllUnknowns);
            }
        }

        /// <summary>
        /// Toon's level.
        /// </summary>
        public byte Level
        {
            get
            {
                return DBToon.Level;
            }
            private set
            {
                this.DBToon.Level = value;
            }
        }

        /// <summary>
        /// Experience to next level
        /// </summary>
        public int ExperienceNext { get; set; }

        /// <summary>
        /// Total time played for toon.
        /// </summary>
        public uint TimePlayed { get { return this.DBToon.TimePlayed; } set { this.DBToon.TimePlayed = value; } }

        /// <summary>
        /// Last login time for toon.
        /// </summary>
        public uint LoginTime { get; set; }

        /// <summary>
        /// Settings for toon.
        /// </summary>
        private D3.Client.ToonSettings _settings = D3.Client.ToonSettings.CreateBuilder().Build();
        public D3.Client.ToonSettings Settings
        {
            get
            {
                return this._settings;
            }
            set
            {
                this._settings = value;
            }
        }

        /// <summary>
        /// Toon digest.
        /// </summary>
        public D3.Hero.Digest Digest
        {
            get
            {
                return D3.Hero.Digest.CreateBuilder().SetVersion(902)
                                .SetHeroId(this.D3EntityID)
                                .SetHeroName(this.Name)
                                .SetGbidClass((int)this.ClassID)
                                .SetPlayerFlags((uint)this.Flags)
                                .SetLevel(this.Level)
                                .SetVisualEquipment(this.HeroVisualEquipmentField.Value)
                                .SetLastPlayedAct(this.ActiveAct)
                                .SetHighestUnlockedAct(0)
                                .SetLastPlayedDifficulty(0)
                                .SetHighestUnlockedDifficulty(0)
                                .SetAltLevel(this.AltLevel)
                                .SetLastPlayedQuest(this.ActiveQuest)
                                .SetLastPlayedQuestStep(this.StepIDofQuest)
                                .SetTimePlayed(this.TimePlayed)
                                .Build();
            }
        }

        /// <summary>
        /// Hero Profile.
        /// </summary>
        public D3.Profile.HeroProfile Profile
        {
            get
            {
                var HeroEquipment = DBSessions.AccountSession.Query<DBInventory>().Where(inv => inv.DBItemInstance != null && inv.DBToon.Id == this.DBToon.Id && inv.EquipmentSlot != -1).ToList();
                var Equipment = D3.Items.ItemList.CreateBuilder();

                #region Формирование Скиллов
                var PlayerSetOfSkills = new GS.Skills.SkillSet(Class, this);
                var Skills = D3.Profile.SkillsWithRunes.CreateBuilder();
                var Passive = D3.Profile.PassiveSkills.CreateBuilder();
                foreach (var Skill in PlayerSetOfSkills.ActiveSkills)
                {
                    if (Skill.snoSkill != -1)
                        Skills.AddRunes(D3.Profile.SkillWithRune.CreateBuilder().SetSkill(Skill.snoSkill).SetRuneType(Skill.snoRune));

                }
                foreach (var PassSkill in PlayerSetOfSkills.PassiveSkills)
                {
                    if (PassSkill != -1)
                        Passive.AddSnoTraits(PassSkill);
                }
                #endregion

                #region Формирование характеристик (ver 0.1)

                #region Основные характеристики
                float Strength;

                if (HeroTable.CoreAttribute == PrimaryAttribute.Strength)
                    Strength = HeroTable.Strength + ((this.Level - 1) * 3);
                else
                    Strength = HeroTable.Strength + (this.Level - 1);

                float Dex;
                if (HeroTable.CoreAttribute == PrimaryAttribute.Dexterity)
                    Dex = HeroTable.Dexterity + ((this.Level - 1) * 3);
                else
                    Dex = HeroTable.Dexterity + (this.Level - 1);

                float Vit = HeroTable.Vitality + ((this.Level - 1) * 2);

                float Intell;
                if (HeroTable.CoreAttribute == PrimaryAttribute.Intelligence)
                    Intell = HeroTable.Intelligence + ((this.Level - 1) * 3);
                else
                    Intell = HeroTable.Intelligence + (this.Level - 1);
                #endregion


                float Armor = 0f + Strength;
                float CalcDamage = (int)HeroTable.CoreAttribute;

                foreach (var inv in HeroEquipment)
                {
                    float minDamage = 0f;
                    float deltaDamage = 0f;
                    float speedDamage = 0f;
                    float Bonus_Strength = 0f;
                    float Bonus_Dexterity = 0f;
                    float Bonus_Intelligence = 0f;
                    float Bonus_Vitality = 0f;

                    if (inv.EquipmentSlot != 0)
                    {
                        var pairs = inv.DBItemInstance.Attributes.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);



                        foreach (var pair in pairs)
                        {
                            var pairParts = pair.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                            if (pairParts.Length != 2)
                            {
                                Logger.Error("GA Deserializated error, skipping Bad Pair.");
                                continue;
                            }
                            var values = pairParts[1].Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                            var valueI = int.Parse(values[0].Trim());
                            var valueF = 0.0f;
                            if (!float.TryParse(values[1].Trim(), out valueF))
                            {
                                Logger.Error("Error Parsing ValueF");
                            }

                            var keyData = pairParts[0].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            var attributeId = int.Parse(keyData[0].Trim());
                            var gameAttribute = GameAttribute.Attributes[attributeId];// .GetById(attributeId);

                            if (gameAttribute.ScriptFunc != null && !gameAttribute.ScriptedAndSettable)
                                continue;
                            int? attributeKey = null;
                            if (keyData.Length > 1)
                            {
                                attributeKey = int.Parse(keyData[1].Trim());
                            }


                            //Калькулируем урон
                            /*
                            if (keyData[0] == "196")//Damage_weapon_min
                                minDamage = valueF;
                            if (keyData[0] == "188")//Damage_weapon_Delta
                                deltaDamage = valueF;
                            if (keyData[0] == "172")//Attacks_Per_Second_Item
                                speedDamage = valueF;

                            if (keyData[0] == "820")//Strength_Item
                                Bonus_Strength = valueF;
                            if (keyData[0] == "821")//Dexterity_Item
                                Bonus_Dexterity = valueF;
                            if (keyData[0] == "822")//Intelligence_Item
                                Bonus_Intelligence = valueF;
                            if (keyData[0] == "823")//Vitality_Item
                                Bonus_Vitality = valueF;
                            */
                            //Добавляем броню
                            if (keyData[0] == "48")
                                Armor += valueF;
                            //var val = RawGetAttributeValue(gameAttribute, attributeKey);
                            //val.ValueF = valueF;
                            //val.Value = valueI;
                            //RawSetAttributeValue(gameAttribute, attributeKey, val);
                        }


                    }

                    if (minDamage != 0f & deltaDamage != 0f & speedDamage != 0f)
                    {
                        CalcDamage += ((minDamage * 2 + deltaDamage) / 2) * speedDamage;
                    }
                    Strength += Bonus_Strength;
                    Dex += Bonus_Dexterity;
                    Intell += Bonus_Intelligence;
                    Vit += Bonus_Vitality;
                }


                #endregion

                #region Шмотки)

                foreach (var inv in HeroEquipment)
                {
                    float Durability_Cur = 0;
                    float Durability_Max = 0;
                    float ItemQuality = 0;
                    float Seed = 0;
                    if (inv.EquipmentSlot != 0)
                    {
                        var pairs = inv.DBItemInstance.Attributes.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                        var affixListStr = inv.DBItemInstance.Affixes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        var affixList = affixListStr.Select(int.Parse).Select(affixId => new GS.Items.Affix(affixId)).ToList();

                        foreach (var pair in pairs)
                        {
                            var pairParts = pair.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            if (pairParts.Length != 2)
                            {
                                Logger.Error("GA Deserializated error, skipping Bad Pair.");
                                continue;
                            }
                            var values = pairParts[1].Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                            var valueI = int.Parse(values[0].Trim());
                            var valueF = 0.0f;
                            if (!float.TryParse(values[1].Trim(), out valueF))
                            {
                                Logger.Error("Error Parsing ValueF");
                            }
                            var keyData = pairParts[0].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            var attributeId = int.Parse(keyData[0].Trim());
                            var gameAttribute = GameAttribute.Attributes[attributeId];// .GetById(attributeId);
                            if (gameAttribute.ScriptFunc != null && !gameAttribute.ScriptedAndSettable)
                                continue;
                            int? attributeKey = null;
                            if (keyData.Length > 1)
                                attributeKey = int.Parse(keyData[1].Trim());

                            if (keyData[0] == "330")//Семя
                                Seed = valueF;
                            if (keyData[0] == "309")//Текущая прочность
                                Durability_Cur = valueF;
                            if (keyData[0] == "310")//Максимальная прочность
                                Durability_Max = valueF;
                            if (keyData[0] == "312")//Максимальная прочность
                                ItemQuality = valueF;
                        }
                        var TestHandle = D3.GameBalance.Handle.CreateBuilder()
                                            .SetGameBalanceType((int)GBHandleType.Gizmo)
                                            .SetGbid(inv.DBItemInstance.GbId)
                                            ;

                        var TestGenerator = D3.Items.Generator.CreateBuilder()
                                                    //                                                    .SetSeed((uint)Seed)
                                                    .SetSeed((uint)RandomHelper.Next())
                                                    .SetGbHandle(TestHandle)
                                                    .SetDurability((uint)Durability_Cur)
                                                    .SetMaxDurability((uint)Durability_Max)
                                                    .SetStackSize(1)
                                                    .SetFlags(0x1)
                                                    .SetItemQualityLevel((int)ItemQuality)
                                                    .SetDyeType(0)
                                                    ;

                        //Добавляем аффиксы
                        foreach (var affix in affixList)
                        {
                            TestGenerator.AddBaseAffixes(affix.AffixGbid);
                        }

                        var ItemId = D3.OnlineService.ItemId.CreateBuilder()
                                                    .SetIdHigh(0)
                                                    .SetIdLow(inv.Id)
                                                    ;

                        var TestItem = D3.Items.SavedItem.CreateBuilder().SetOwnerEntityId(this.D3EntityID)
                                .SetGenerator(TestGenerator)
                                .SetItemSlot(inv.EquipmentSlot)
                                //.SetSocketId(D3.OnlineService.ItemId.CreateBuilder().SetIdHigh(0).SetIdLow(0))
                                .SetSquareIndex(-1)
                                .SetHirelingClass(4482)
                                .SetId(ItemId)
                                .SetUsedSocketCount(0)
                                .SetOwnerEntityId(this.D3EntityID)
                                ;

                        Equipment.AddItems(TestItem);
                    }
                }

                var StoneOfRecallSkellet = D3.Items.EmbeddedGenerator.CreateBuilder();
                //StoneOfRecallSkellet.Set





                #endregion

                return D3.Profile.HeroProfile.CreateBuilder()
                    .SetHardcore(this.Hardcore)
                    .SetHeroId(this.D3EntityID)
                    .SetHighestDifficulty(3)
                    .SetHighestLevel(this.Level)
                    .SetSnoActiveSkills(Skills)
                    .SetSnoTraits(Passive)
                    .SetStrength((uint)Strength)//Сила
                    .SetDexterity((uint)Dex)//Ловкость
                    .SetIntelligence((uint)Intell)//Интеллект
                    .SetVitality((uint)Vit)//Живучесть
                    .SetEquipment(Equipment)
                    .SetArmor((uint)Armor)
                    .SetDps(CalcDamage)

                    .Build();
            }
        }

        public bool IsSelected
        {
            get
            {
                if (!this.GameAccount.IsOnline) return false;
                else
                {
                    if (this.GameAccount.CurrentToon != null)
                        return this.GameAccount.CurrentToon == this;
                    else
                        return false;
                }
            }
        }

        public int ClassID
        {
            get
            {
                switch (this.Class)
                {
                    case ToonClass.Barbarian:
                        return 0x4FB91EE2;
                    case ToonClass.DemonHunter:
                        return unchecked((int)0xC88B9649);
                    case ToonClass.Monk:
                        return 0x3DAC15;
                    case ToonClass.WitchDoctor:
                        return 0x343C22A;
                    case ToonClass.Wizard:
                        return 0x1D4681B1;
                }
                return 0x0;
            }
        }

        public int VoiceClassID // Used for Conversations
        {
            get
            {
                switch (this.Class)
                {
                    case ToonClass.DemonHunter:
                        return 0;
                    case ToonClass.Barbarian:
                        return 1;
                    case ToonClass.Wizard:
                        return 2;
                    case ToonClass.WitchDoctor:
                        return 3;
                    case ToonClass.Monk:
                        return 4;
                }
                return 0;
            }
        }

        public int Gender
        {
            get
            {
                return (int)(this.Flags & ToonFlags.Female); // 0x00 for male, so we can just return the AND operation
            }
        }

        #region c-tor and setfields

        public readonly HeroTable HeroTable;
        private static readonly NullD.Common.MPQ.FileFormats.GameBalance HeroData = (NullD.Common.MPQ.FileFormats.GameBalance)MPQStorage.Data.Assets[NullD.Core.GS.Common.Types.SNO.SNOGroup.GameBalance][19740].Data;

        public Toon(DBToon dbToon)
            : base(dbToon.Id)
        {
            this.D3EntityID = D3.OnlineService.EntityId.CreateBuilder().SetIdHigh((ulong)EntityIdHelper.HighIdType.ToonId).SetIdLow(this.PersistentID).Build();

            this.DBToon = dbToon;
            this.HeroTable = HeroData.Heros.Find(item => item.Name == this.Class.ToString());
            this.ExperienceNext = Player.LevelBorders[this.Level];

            var visualItems = new[]
            {
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Head
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Chest
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Feet
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Hands
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Weapon (1)
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Weapon (2)
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Shoulders
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Legs
            };

            // Load Visual Equipment
            var visualToSlotMapping = new Dictionary<int, int> { { 1, 0 }, { 2, 1 }, { 7, 2 }, { 5, 3 }, { 4, 4 }, { 3, 5 }, { 8, 6 }, { 9, 7 } };

            //add visual equipment from DB, only the visualizable equipment, not everything
            var visibleEquipment = DBSessions.AccountSession.Query<DBInventory>().Where(inv => inv.DBItemInstance != null && inv.DBToon.Id == dbToon.Id && inv.EquipmentSlot != -1).ToList();

            foreach (var inv in visibleEquipment)
            {
                var slot = inv.EquipmentSlot;
                if (!visualToSlotMapping.ContainsKey(slot))
                    continue;
                // decode vislual slot from equipment slot
                slot = visualToSlotMapping[slot];
                var gbid = inv.DBItemInstance.GbId;
                visualItems[slot] = D3.Hero.VisualItem.CreateBuilder()
                    .SetGbid(gbid)
                    .SetEffectLevel(0)
                    .Build();
            }

            this.HeroVisualEquipmentField.Value = D3.Hero.VisualEquipment.CreateBuilder().AddRangeVisualItem(visualItems).Build();
        }

        /* old non-db toon creation ctor. /raist.
        public Toon(string name, int hashCode, int classId, ToonFlags flags, byte level, GameAccount account) // Toon with **newly generated** persistent ID
            : base(StringHashHelper.HashIdentity(name + "#" + hashCode.ToString("D3")))
        {
            this.D3EntityID = D3.OnlineService.EntityId.CreateBuilder().SetIdHigh((ulong)EntityIdHelper.HighIdType.ToonId).SetIdLow(this.PersistentID).Build();

            this.Name = name;
            this.HashCode = hashCode;
            this.Class = @GetClassByID(classId);
            this.Flags = flags;
            this.Level = level;
            this.ExperienceNext = Player.LevelBorders[level];
            this.GameAccount = account;
            this.TimePlayed = 0;

            var visualItems = new[]
            {                                
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Head
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Chest
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Feet
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Hands
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Weapon (1)
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Weapon (2)
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Shoulders
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Legs
            };

            this.HeroVisualEquipmentField.Value = D3.Hero.VisualEquipment.CreateBuilder().AddRangeVisualItem(visualItems).Build();
        }

        public Toon(ulong persistentId)     // Load a toon from database with a given persistentId
            : base(persistentId)
        {
            this.D3EntityID = D3.OnlineService.EntityId.CreateBuilder().SetIdHigh((ulong)EntityIdHelper.HighIdType.ToonId).SetIdLow(this.PersistentID).Build();

            var sqlQuery  = string.Format("SELECT * FROM toons WHERE id = {0}", persistentId);
            var sqlCmd    = new SQLiteCommand(sqlQuery, DBManager.Connection);
            var sqlReader = sqlCmd.ExecuteReader();

            // Use name of column to prevent errors if column moved
            while (sqlReader.Read())
            {
                this.Name = Convert.ToString(sqlReader["name"]);
                this.HashCode = Convert.ToInt32(sqlReader["hashCode"]);
                this.Class = (ToonClass)Convert.ToInt32(sqlReader["class"]);
                this.Flags = (ToonFlags)Convert.ToInt32(sqlReader["gender"]);
                this.Level = Convert.ToByte(sqlReader["level"]);
                this.ExperienceNext = Convert.ToInt32(sqlReader["experience"]);
                this.GameAccount = GameAccountManager.GetAccountByPersistentID(Convert.ToUInt64(sqlReader["accountId"]));
                this.TimePlayed = Convert.ToUInt32(sqlReader["timePlayed"]);
                this.Deleted = Convert.ToBoolean(sqlReader["deleted"]);
            }

            var visualItems = new[]
            {                                
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Head
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Chest
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Feet
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Hands
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Weapon (1)
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Weapon (2)
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Shoulders
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Legs
            };
            
            // Load Visual Equipment
            Dictionary<int, int> visualToSlotMapping = new Dictionary<int, int>();
            visualToSlotMapping.Add(1, 0);
            visualToSlotMapping.Add(2, 1);
            visualToSlotMapping.Add(7, 2);
            visualToSlotMapping.Add(5, 3);
            visualToSlotMapping.Add(4, 4);
            visualToSlotMapping.Add(3, 5);
            visualToSlotMapping.Add(8, 6);
            visualToSlotMapping.Add(9, 7);
            
            //add visual equipment form DB, only the visualizable equipment, not everything
            var itemQuery = string.Format("SELECT * FROM inventory WHERE toon_id = {0} AND equipment_slot <> -1 AND item_id <> -1", persistentId);
            var itemCmd = new SQLiteCommand(itemQuery, DBManager.Connection);
            var itemReader = itemCmd.ExecuteReader();
            if (itemReader.HasRows)
            {
                while (itemReader.Read())
                {
                    var slot = Convert.ToInt32(itemReader["equipment_slot"]);
                    if (!visualToSlotMapping.ContainsKey(slot))
                        continue;
                    // decode vislual slot from equipment slot
                    slot = visualToSlotMapping[slot];
                    var gbid = Convert.ToInt32(itemReader["item_id"]);
                    visualItems[slot] = D3.Hero.VisualItem.CreateBuilder()
                        .SetGbid(gbid)
                        .SetEffectLevel(0)
                        .Build();
                }
            }
            this.HeroVisualEquipmentField.Value = D3.Hero.VisualEquipment.CreateBuilder().AddRangeVisualItem(visualItems).Build();
        }
        */
        #endregion

        public void LevelUp()
        {
            this.Level++;
            this.GameAccount.ChangedFields.SetIntPresenceFieldValue(this.HeroLevelField);
        }

        #region Notifications

        //hero class generated
        //D3,Hero,1,0 -> D3.Hero.GbidClass: Hero Class
        //D3,Hero,2,0 -> D3.Hero.Level: Hero's current level
        //D3,Hero,3,0 -> D3.Hero.VisualEquipment: VisualEquipment
        //D3,Hero,4,0 -> D3.Hero.PlayerFlags: Hero's flags
        //D3,Hero,5,0 -> ?D3.Hero.NameText: Hero's Name
        //D3,Hero,6,0 -> Unk Int64 (0)
        //D3,Hero,7,0 -> Unk Int64 (0)

        public override List<bnet.protocol.presence.FieldOperation> GetSubscriptionNotifications()
        {
            var operationList = new List<bnet.protocol.presence.FieldOperation>();
            operationList.Add(this.HeroClassField.GetFieldOperation());
            operationList.Add(this.HeroLevelField.GetFieldOperation());
            operationList.Add(this.HeroVisualEquipmentField.GetFieldOperation());
            operationList.Add(this.HeroFlagsField.GetFieldOperation());
            operationList.Add(this.HeroNameField.GetFieldOperation());
            operationList.Add(this.HighestUnlockedAct.GetFieldOperation());
            operationList.Add(this.HighestUnlockedDifficulty.GetFieldOperation());

            return operationList;
        }

        #endregion

        public static ToonClass GetClassByID(int classId)
        {
            switch (classId)
            {
                case 0x4FB91EE2:
                    return ToonClass.Barbarian;
                case unchecked((int)0xC88B9649):
                    return ToonClass.DemonHunter;
                case 0x003DAC15:
                    return ToonClass.Monk;
                case 0x0343C22A:
                    return ToonClass.WitchDoctor;
                case 0x1D4681B1:
                    return ToonClass.Wizard;
            }

            return ToonClass.Barbarian;
        }

        public override string ToString()
        {
            return String.Format("{{ Toon: {0} [lowId: {1}] }}", this.Name, this.D3EntityID.IdLow);
        }

        #region DB


        /*
        private bool VisualItemExistsInDb(int slot)
        {
            var query = string.Format("SELECT toon_id FROM inventory WHERE toon_id = {0} AND equipment_slot = {1}", this.PersistentID, slot);
            var cmd = new SQLiteCommand(query, DBManager.Connection);
            var reader = cmd.ExecuteReader();
            return reader.HasRows;
        }*/
    }
        #endregion

    #region Definitions and Enums
    //Order is important as actor voices and saved data is based on enum index
    public enum ToonClass// : uint
    {
        Barbarian,// = 0x4FB91EE2,
        Monk,//= 0x3DAC15,
        DemonHunter,// = 0xC88B9649,
        WitchDoctor,// = 0x343C22A,
        Wizard,// = 0x1D4681B1
    }

    [Flags]
    public enum ToonFlags : uint
    {
        Male = 0x00,
        Female = 0x02,
        // TODO: These two need to be figured out still.. /plash
        //Unknown1 = 0x20,
        Unknown2 = 0x40,
        Unknown3 = 0x80000,
        Unknown4 = 0x2000000,
        AllUnknowns = Unknown2 | Unknown3 | Unknown4
    }
    #endregion
}
