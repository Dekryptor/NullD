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
using System.Data.SQLite;
using System.Linq;
using NullD.Common.Logging;
using NullD.Common.Storage;
using NullD.Common.Storage.AccountDataBase.Entities;
using NullD.Core.LogNet.Accounts;
using NHibernate.Linq;

namespace NullD.Core.LogNet.Toons
{
    // Just a quick hack - not to be meant final
    public static class ToonManager
    {
        private static readonly HashSet<Toon> LoadedToons = new HashSet<Toon>();
        private static readonly Logger Logger = LogManager.CreateLogger();


        public static Toon GetToonByDBToon(DBToon dbToon)
        {
            if (!LoadedToons.Any(dbt => dbt.DBToon.Id == dbToon.Id))
                LoadedToons.Add(new Toon(dbToon));
            return LoadedToons.Single(dbt => dbt.DBToon.Id == dbToon.Id);
        }


        public static Account GetOwnerAccountByToonLowId(ulong id)
        {
            return GetToonByLowID(id).GameAccount.Owner;
        }

        public static GameAccount GetOwnerGameAccountByToonLowId(ulong id)
        {
            return GetToonByLowID(id).GameAccount;
        }



        public static Toon GetToonByLowID(ulong id)
        {
            var dbToon = DBSessions.AccountSession.Get<DBToon>(id);
            return GetToonByDBToon(dbToon);
        }

        public static Toon GetDeletedToon(GameAccount account)
        {
            var query = DBSessions.AccountSession.Query<DBToon>().Where(dbt => dbt.DBGameAccount.Id == account.PersistentID && dbt.Deleted);
            return query.Any() ? GetToonByLowID(query.First().Id) : null;
        }

        public static List<Toon> GetToonsForGameAccount(GameAccount account)
        {
            var toons = account.DBGameAccount.DBToons.Select(dbt => GetToonByLowID(dbt.Id));
            return toons.ToList();
        }


        public static int TotalToons
        {
            get { return DBSessions.AccountSession.Query<DBToon>().Count(); }
        }


        public static Toon CreateNewToon(string name, int classId, ToonFlags flags, byte level, GameAccount gameAccount)
        {

            #region LevelConfs Server.conf
            if (NullD.Net.GS.Config.Instance.LevelStarter < 1)
                level = 1;
            else if (NullD.Net.GS.Config.Instance.LevelStarter > NullD.Net.GS.Config.Instance.MaxLevel)
                level = (byte)NullD.Net.GS.Config.Instance.MaxLevel;
            else
                level = (byte)NullD.Net.GS.Config.Instance.LevelStarter;
            #endregion

            var dbGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID);
            var newDBToon = new DBToon
                                {
                                    Class = @Toon.GetClassByID(classId),
                                    Name = name,
                                    /*HashCode = GetUnusedHashCodeForToonName(name),*/
                                    Flags = flags,
                                    Level = level,
                                    DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID)
                                };
            dbGameAccount.DBToons.Add(newDBToon);

            #region Стандартные предметы 1.0.4
            //*
            #region Стандартные предметы Barbarian
            if (newDBToon.Class == ToonClass.Barbarian)
            {
                #region Топор
                //Структура сущности предмета
                DBItemInstance BARAxeInstance = new DBItemInstance
                {
                    GbId = 1661412390, // 
                    Affixes = "",
                    Attributes = "312,:4|5,605194E-45;101,:0|0;329,:1|1,401298E-45;196,0:1077936128|3;190,0:1082130432|4;197,0:1077936128|3;191,0:1082130432|4;192,:1082130432|4;198,:1077936128|3;201,:1080033280|3,5;200,0:1080033280|3,5;434,0:1077936128|3;439,0:1077936128|3;186,0:1077936128|3;182,0:1077936128|3;184,:1077936128|3;185,:1080033280|3,5;435,0:0|0;199,0:1080033280|3,5;188,0:1065353216|1;189,0:1065353216|1;193,0:1065353216|1;194,:1065353216|1;436,0:1065353216|1;440,0:1065353216|1;179,0:1065353216|1;183,:1065353216|1;437,0:0|0;326,:1|1,401298E-45;330,:851683752|2,277814E-08;167,:1067869798|1,3;169,:1067869798|1,3;171,:1067869798|1,3;430,:1067869798|1,3;432,:1067869798|1,3;438,:1067869798|1,3;174,:1067869798|1,3;344,:0|0;345,:0|0;346,:0|0;347,:0|0;431,:0|0;433,:0|0;99,30592:1|1,401298E-45;100,30592:1|1,401298E-45;315,:1|1,401298E-45"
                };
                // Добавляем сущность в базу
                DBSessions.AccountSession.SaveOrUpdate(BARAxeInstance);
                // Структура предмета
                DBInventory BARAxeFirstWeapon = new DBInventory
                {
                    DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID), // Привязка к используемому аккаунту
                    DBItemInstance = BARAxeInstance, // Использовать свеже созданную сущность предмета
                    //Hardcore = isHardcore, // Хардкорный или нет персонаж
                    DBToon = newDBToon, // Выдать созданному персонажу
                    EquipmentSlot = 4, // Вооружить в первую руку
                    LocationX = 0,
                    LocationY = 0
                };
                // Добавляем предмет в базу
                DBSessions.AccountSession.SaveOrUpdate(BARAxeFirstWeapon);
                #endregion

                #region Нагрудник
                //Структура сущности предмета
                DBItemInstance ChestInstance = new DBItemInstance
                {
                    GbId = 1612257704, // 
                    Affixes = "",
                    Attributes = "312,:1|1,401298E-45;101,:0|0;329,:1|1,401298E-45;48,:1073741824|2;51,:1073741824|2;52,:1073741824|2;53,:1073741824|2;326,:1|1,401298E-45;330,:1425612098|8,559502E+12"
                };
                // Добавляем сущность в базу
                DBSessions.AccountSession.SaveOrUpdate(ChestInstance);
                // Структура предмета
                DBInventory BARChest = new DBInventory
                {
                    DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID), // Привязка к используемому аккаунту
                    DBItemInstance = ChestInstance, // Использовать свеже созданную сущность предмета
                    //Hardcore = isHardcore, // Хардкорный или нет персонаж
                    DBToon = newDBToon, // Выдать созданному персонажу
                    EquipmentSlot = 2, // Вооружить в первую руку
                    LocationX = 0,
                    LocationY = 0
                };
                // Добавляем предмет в базу
                DBSessions.AccountSession.SaveOrUpdate(BARChest);
                #endregion

                #region Штаны
                //Структура сущности предмета
                DBItemInstance PantsInstance = new DBItemInstance
                {
                    GbId = -1512732138, // 
                    Affixes = "",
                    Attributes = "312,:0|0;101,:0|0;329,:1|1,401298E-45;48,:1084227584|5;51,:1084227584|5;52,:1084227584|5;53,:1084227584|5;326,:1|1,401298E-45;330,:56548088|6,54913E-37;315,:1|1,401298E-45"
                };
                // Добавляем сущность в базу
                DBSessions.AccountSession.SaveOrUpdate(PantsInstance);
                // Структура предмета
                DBInventory BARPants = new DBInventory
                {
                    DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID), // Привязка к используемому аккаунту
                    DBItemInstance = PantsInstance, // Использовать свеже созданную сущность предмета
                    //Hardcore = isHardcore, // Хардкорный или нет персонаж
                    DBToon = newDBToon, // Выдать созданному персонажу
                    EquipmentSlot = 9, // Вооружить в первую руку
                    LocationX = 0,
                    LocationY = 0
                };
                // Добавляем предмет в базу
                DBSessions.AccountSession.SaveOrUpdate(BARPants);
                #endregion
            }
            #endregion

            #region Стандартные предметы DemonHunter
            else if (newDBToon.Class == ToonClass.DemonHunter)
            {
                #region Одноручный арбалет
                //Структура сущности предмета
                DBItemInstance DHbowInstance = new DBItemInstance
                {
                    GbId = -2091504072, // 
                    Affixes = "",
                    Attributes = "312,:4|5,605194E-45;101,:0|0;329,:1|1,401298E-45;196,0:1077936128|3;190,0:1082130432|4;197,0:1077936128|3;191,0:1082130432|4;192,:1082130432|4;198,:1077936128|3;201,:1080033280|3,5;200,0:1080033280|3,5;434,0:1077936128|3;439,0:1077936128|3;186,0:1077936128|3;182,0:1077936128|3;184,:1077936128|3;185,:1080033280|3,5;435,0:0|0;199,0:1080033280|3,5;188,0:1065353216|1;189,0:1065353216|1;193,0:1065353216|1;194,:1065353216|1;436,0:1065353216|1;440,0:1065353216|1;179,0:1065353216|1;183,:1065353216|1;437,0:0|0;326,:1|1,401298E-45;330,:851683752|2,277814E-08;167,:1067869798|1,3;169,:1067869798|1,3;171,:1067869798|1,3;430,:1067869798|1,3;432,:1067869798|1,3;438,:1067869798|1,3;174,:1067869798|1,3;344,:0|0;345,:0|0;346,:0|0;347,:0|0;431,:0|0;433,:0|0;99,30592:1|1,401298E-45;100,30592:1|1,401298E-45;315,:1|1,401298E-45"
                };
                // Добавляем сущность в базу
                DBSessions.AccountSession.SaveOrUpdate(DHbowInstance);
                // Структура предмета
                DBInventory DHFirstWeapon = new DBInventory
                {
                    DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID), // Привязка к используемому аккаунту
                    DBItemInstance = DHbowInstance, // Использовать свеже созданную сущность предмета
                    //Hardcore = isHardcore, // Хардкорный или нет персонаж
                    DBToon = newDBToon, // Выдать созданному персонажу
                    EquipmentSlot = 4, // Вооружить в первую руку
                    LocationX = 0,
                    LocationY = 0
                };
                // Добавляем предмет в базу
                DBSessions.AccountSession.SaveOrUpdate(DHFirstWeapon);
                #endregion

                #region Нагрудник
                //Структура сущности предмета
                DBItemInstance ChestInstance = new DBItemInstance
                {
                    GbId = 1612257704, // 
                    Affixes = "",
                    Attributes = "312,:1|1,401298E-45;101,:0|0;329,:1|1,401298E-45;48,:1073741824|2;51,:1073741824|2;52,:1073741824|2;53,:1073741824|2;326,:1|1,401298E-45;330,:1425612098|8,559502E+12"
                };
                // Добавляем сущность в базу
                DBSessions.AccountSession.SaveOrUpdate(ChestInstance);
                // Структура предмета
                DBInventory BARChest = new DBInventory
                {
                    DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID), // Привязка к используемому аккаунту
                    DBItemInstance = ChestInstance, // Использовать свеже созданную сущность предмета
                   // Hardcore = isHardcore, // Хардкорный или нет персонаж
                    DBToon = newDBToon, // Выдать созданному персонажу
                    EquipmentSlot = 2, // Вооружить в первую руку
                    LocationX = 0,
                    LocationY = 0
                };
                // Добавляем предмет в базу
                DBSessions.AccountSession.SaveOrUpdate(BARChest);
                #endregion

                #region Штаны
                //Структура сущности предмета
                DBItemInstance PantsInstance = new DBItemInstance
                {
                    GbId = -1512732138, // 
                    Affixes = "",
                    Attributes = "312,:0|0;101,:0|0;329,:1|1,401298E-45;48,:1084227584|5;51,:1084227584|5;52,:1084227584|5;53,:1084227584|5;326,:1|1,401298E-45;330,:56548088|6,54913E-37;315,:1|1,401298E-45"
                };
                // Добавляем сущность в базу
                DBSessions.AccountSession.SaveOrUpdate(PantsInstance);
                // Структура предмета
                DBInventory BARPants = new DBInventory
                {
                    DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID), // Привязка к используемому аккаунту
                    DBItemInstance = PantsInstance, // Использовать свеже созданную сущность предмета
                    //Hardcore = isHardcore, // Хардкорный или нет персонаж
                    DBToon = newDBToon, // Выдать созданному персонажу
                    EquipmentSlot = 9, // Вооружить в первую руку
                    LocationX = 0,
                    LocationY = 0
                };
                // Добавляем предмет в базу
                DBSessions.AccountSession.SaveOrUpdate(BARPants);
                #endregion
            }
            #endregion

            #region Стандартные предметы Monk
            else if (newDBToon.Class == ToonClass.Monk)
            {
                #region Кастет 1
                //Структура сущности предмета
                DBItemInstance DHbowInstance = new DBItemInstance
                {
                    GbId = 1236604967, // 
                    Affixes = "",
                    Attributes = "312,:2|2,802597E-45;101,:0|0;329,:1|1,401298E-45;196,0:1065353216|1;190,0:1077936128|3;197,0:1065353216|1;191,0:1077936128|3;192,:1077936128|3;198,:1065353216|1;201,:1073741824|2;200,0:1073741824|2;434,0:1065353216|1;439,0:1065353216|1;186,0:1065353216|1;182,0:1065353216|1;184,:1065353216|1;185,:1073741824|2;435,0:0|0;199,0:1073741824|2;188,0:1073741824|2;189,0:1073741824|2;193,0:1073741824|2;194,:1073741824|2;436,0:1073741824|2;440,0:1073741824|2;179,0:1073741824|2;183,:1073741824|2;437,0:0|0;326,:1|1,401298E-45;330,:750642467|5,397142E-12;167,:1067030938|1,2;169,:1067030938|1,2;171,:1067030938|1,2;430,:1067030938|1,2;432,:1067030938|1,2;438,:1067030938|1,2;174,:1067030938|1,2;344,:0|0;345,:0|0;346,:0|0;347,:0|0;431,:0|0;433,:0|0;99,30592:1|1,401298E-45;100,30592:1|1,401298E-45;315,:1|1,401298E-45"
                };
                // Добавляем сущность в базу
                DBSessions.AccountSession.SaveOrUpdate(DHbowInstance);
                // Структура предмета
                DBInventory DHFirstWeapon = new DBInventory
                {
                    DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID), // Привязка к используемому аккаунту
                    DBItemInstance = DHbowInstance, // Использовать свеже созданную сущность предмета
                    //Hardcore = isHardcore, // Хардкорный или нет персонаж
                    DBToon = newDBToon, // Выдать созданному персонажу
                    EquipmentSlot = 4, // Вооружить в первую руку
                    LocationX = 0,
                    LocationY = 0
                };
                // Добавляем предмет в базу
                DBSessions.AccountSession.SaveOrUpdate(DHFirstWeapon);
                #endregion

                #region Кастет 2
                //Структура сущности предмета
                DBItemInstance DHbowInstance2 = new DBItemInstance
                {
                    GbId = 1236604967, // 
                    Affixes = "",
                    Attributes = "312,:2|2,802597E-45;101,:0|0;329,:1|1,401298E-45;196,0:1065353216|1;190,0:1077936128|3;197,0:1065353216|1;191,0:1077936128|3;192,:1077936128|3;198,:1065353216|1;201,:1073741824|2;200,0:1073741824|2;434,0:1065353216|1;439,0:1065353216|1;186,0:1065353216|1;182,0:1065353216|1;184,:1065353216|1;185,:1073741824|2;435,0:0|0;199,0:1073741824|2;188,0:1073741824|2;189,0:1073741824|2;193,0:1073741824|2;194,:1073741824|2;436,0:1073741824|2;440,0:1073741824|2;179,0:1073741824|2;183,:1073741824|2;437,0:0|0;326,:1|1,401298E-45;330,:750642467|5,397142E-12;167,:1067030938|1,2;169,:1067030938|1,2;171,:1067030938|1,2;430,:1067030938|1,2;432,:1067030938|1,2;438,:1067030938|1,2;174,:1067030938|1,2;344,:0|0;345,:0|0;346,:0|0;347,:0|0;431,:0|0;433,:0|0;99,30592:1|1,401298E-45;100,30592:1|1,401298E-45;315,:1|1,401298E-45"
                };
                // Добавляем сущность в базу
                DBSessions.AccountSession.SaveOrUpdate(DHbowInstance2);
                // Структура предмета
                DBInventory DHFirstWeapon2 = new DBInventory
                {
                    DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID), // Привязка к используемому аккаунту
                    DBItemInstance = DHbowInstance2, // Использовать свеже созданную сущность предмета
                    //Hardcore = isHardcore, // Хардкорный или нет персонаж
                    DBToon = newDBToon, // Выдать созданному персонажу
                    EquipmentSlot = 3, // Вооружить во вторую руку
                    LocationX = 0,
                    LocationY = 0
                };
                // Добавляем предмет в базу
                DBSessions.AccountSession.SaveOrUpdate(DHFirstWeapon2);
                #endregion

                #region Нагрудник
                //Структура сущности предмета
                DBItemInstance ChestInstance = new DBItemInstance
                {
                    GbId = 1612257704, // 
                    Affixes = "",
                    Attributes = "312,:1|1,401298E-45;101,:0|0;329,:1|1,401298E-45;48,:1073741824|2;51,:1073741824|2;52,:1073741824|2;53,:1073741824|2;326,:1|1,401298E-45;330,:1425612098|8,559502E+12"
                };
                // Добавляем сущность в базу
                DBSessions.AccountSession.SaveOrUpdate(ChestInstance);
                // Структура предмета
                DBInventory BARChest = new DBInventory
                {
                    DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID), // Привязка к используемому аккаунту
                    DBItemInstance = ChestInstance, // Использовать свеже созданную сущность предмета
                    //Hardcore = isHardcore, // Хардкорный или нет персонаж
                    DBToon = newDBToon, // Выдать созданному персонажу
                    EquipmentSlot = 2, // Вооружить в первую руку
                    LocationX = 0,
                    LocationY = 0
                };
                // Добавляем предмет в базу
                DBSessions.AccountSession.SaveOrUpdate(BARChest);
                #endregion

                #region Штаны
                //Структура сущности предмета
                DBItemInstance PantsInstance = new DBItemInstance
                {
                    GbId = -1512732138, // 
                    Affixes = "",
                    Attributes = "312,:0|0;101,:0|0;329,:1|1,401298E-45;48,:1084227584|5;51,:1084227584|5;52,:1084227584|5;53,:1084227584|5;326,:1|1,401298E-45;330,:56548088|6,54913E-37;315,:1|1,401298E-45"
                };
                // Добавляем сущность в базу
                DBSessions.AccountSession.SaveOrUpdate(PantsInstance);
                // Структура предмета
                DBInventory BARPants = new DBInventory
                {
                    DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID), // Привязка к используемому аккаунту
                    DBItemInstance = PantsInstance, // Использовать свеже созданную сущность предмета
                    //Hardcore = isHardcore, // Хардкорный или нет персонаж
                    DBToon = newDBToon, // Выдать созданному персонажу
                    EquipmentSlot = 9, // Вооружить в первую руку
                    LocationX = 0,
                    LocationY = 0
                };
                // Добавляем предмет в базу
                DBSessions.AccountSession.SaveOrUpdate(BARPants);
                #endregion
            }
            #endregion

            #region Стандартные предметы Witch Doctor
            else if (newDBToon.Class == ToonClass.WitchDoctor)
            {
                #region Кинжал
                //Структура сущности предмета
                DBItemInstance DHbowInstance = new DBItemInstance
                {
                    GbId = -1303415302, // 
                    Affixes = "",
                    Attributes = "312,:2|2,802597E-45;101,:0|0;329,:1|1,401298E-45;196,0:1073741824|2;190,0:1088421888|7;197,0:1073741824|2;191,0:1088421888|7;192,:1088421888|7;198,:1073741824|2;201,:1083179008|4,5;200,0:1083179008|4,5;434,0:1073741824|2;439,0:1073741824|2;186,0:1073741824|2;182,0:1073741824|2;184,:1073741824|2;185,:1083179008|4,5;435,0:0|0;199,0:1083179008|4,5;188,0:1084227584|5;189,0:1084227584|5;193,0:1084227584|5;194,:1084227584|5;436,0:1084227584|5;440,0:1084227584|5;179,0:1084227584|5;183,:1084227584|5;437,0:0|0;326,:1|1,401298E-45;330,:432328575|2,034973E-23;167,:1069547520|1,5;169,:1069547520|1,5;171,:1069547520|1,5;430,:1069547520|1,5;432,:1069547520|1,5;438,:1069547520|1,5;174,:1069547520|1,5;344,:0|0;345,:0|0;346,:0|0;347,:0|0;431,:0|0;433,:0|0;99,30592:1|1,401298E-45;100,30592:1|1,401298E-45;315,:1|1,401298E-45"
                };
                // Добавляем сущность в базу
                DBSessions.AccountSession.SaveOrUpdate(DHbowInstance);
                // Структура предмета
                DBInventory DHFirstWeapon = new DBInventory
                {
                    DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID), // Привязка к используемому аккаунту
                    DBItemInstance = DHbowInstance, // Использовать свеже созданную сущность предмета
                   // Hardcore = isHardcore, // Хардкорный или нет персонаж
                    DBToon = newDBToon, // Выдать созданному персонажу
                    EquipmentSlot = 4, // Вооружить в первую руку
                    LocationX = 0,
                    LocationY = 0
                };
                // Добавляем предмет в базу
                DBSessions.AccountSession.SaveOrUpdate(DHFirstWeapon);
                #endregion

                #region Нагрудник
                //Структура сущности предмета
                DBItemInstance ChestInstance = new DBItemInstance
                {
                    GbId = 1612257704, // 
                    Affixes = "",
                    Attributes = "312,:1|1,401298E-45;101,:0|0;329,:1|1,401298E-45;48,:1073741824|2;51,:1073741824|2;52,:1073741824|2;53,:1073741824|2;326,:1|1,401298E-45;330,:1425612098|8,559502E+12"
                };
                // Добавляем сущность в базу
                DBSessions.AccountSession.SaveOrUpdate(ChestInstance);
                // Структура предмета
                DBInventory BARChest = new DBInventory
                {
                    DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID), // Привязка к используемому аккаунту
                    DBItemInstance = ChestInstance, // Использовать свеже созданную сущность предмета
                    //Hardcore = isHardcore, // Хардкорный или нет персонаж
                    DBToon = newDBToon, // Выдать созданному персонажу
                    EquipmentSlot = 2, // Вооружить в первую руку
                    LocationX = 0,
                    LocationY = 0
                };
                // Добавляем предмет в базу
                DBSessions.AccountSession.SaveOrUpdate(BARChest);
                #endregion

                #region Штаны
                //Структура сущности предмета
                DBItemInstance PantsInstance = new DBItemInstance
                {
                    GbId = -1512732138, // 
                    Affixes = "",
                    Attributes = "312,:0|0;101,:0|0;329,:1|1,401298E-45;48,:1084227584|5;51,:1084227584|5;52,:1084227584|5;53,:1084227584|5;326,:1|1,401298E-45;330,:56548088|6,54913E-37;315,:1|1,401298E-45"
                };
                // Добавляем сущность в базу
                DBSessions.AccountSession.SaveOrUpdate(PantsInstance);
                // Структура предмета
                DBInventory BARPants = new DBInventory
                {
                    DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID), // Привязка к используемому аккаунту
                    DBItemInstance = PantsInstance, // Использовать свеже созданную сущность предмета
                    //Hardcore = isHardcore, // Хардкорный или нет персонаж
                    DBToon = newDBToon, // Выдать созданному персонажу
                    EquipmentSlot = 9, // Вооружить в первую руку
                    LocationX = 0,
                    LocationY = 0
                };
                // Добавляем предмет в базу
                DBSessions.AccountSession.SaveOrUpdate(BARPants);
                #endregion
            }
            #endregion

            #region Стандартные предметы Wizard
            else if (newDBToon.Class == ToonClass.Wizard)
            {
                #region Кинжал
                //Структура сущности предмета
                DBItemInstance DHbowInstance = new DBItemInstance
                {
                    GbId = 88665049, // 
                    Affixes = "",
                    Attributes = "312,:5|7,006492E-45;101,:0|0;329,:1|1,401298E-45;196,0:1077936128|3;190,0:1082130432|4;197,0:1077936128|3;191,0:1082130432|4;192,:1082130432|4;198,:1077936128|3;201,:1080033280|3,5;200,0:1080033280|3,5;434,0:1077936128|3;439,0:1077936128|3;186,0:1077936128|3;182,0:1077936128|3;184,:1077936128|3;185,:1080033280|3,5;435,0:0|0;199,0:1080033280|3,5;188,0:1065353216|1;189,0:1065353216|1;193,0:1065353216|1;194,:1065353216|1;436,0:1065353216|1;440,0:1065353216|1;179,0:1065353216|1;183,:1065353216|1;437,0:0|0;326,:1|1,401298E-45;330,:2081659420|3,065573E+36;167,:1067030938|1,2;169,:1067030938|1,2;171,:1067030938|1,2;430,:1067030938|1,2;432,:1067030938|1,2;438,:1067030938|1,2;174,:1067030938|1,2;344,:0|0;345,:0|0;346,:0|0;347,:0|0;431,:0|0;433,:0|0;99,30601:1|1,401298E-45;100,30601:1|1,401298E-45"
                };
                // Добавляем сущность в базу
                DBSessions.AccountSession.SaveOrUpdate(DHbowInstance);
                // Структура предмета
                DBInventory DHFirstWeapon = new DBInventory
                {
                    DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID), // Привязка к используемому аккаунту
                    DBItemInstance = DHbowInstance, // Использовать свеже созданную сущность предмета
                    //Hardcore = isHardcore, // Хардкорный или нет персонаж
                    DBToon = newDBToon, // Выдать созданному персонажу
                    EquipmentSlot = 4, // Вооружить в первую руку
                    LocationX = 0,
                    LocationY = 0
                };
                // Добавляем предмет в базу
                DBSessions.AccountSession.SaveOrUpdate(DHFirstWeapon);
                #endregion

                #region Нагрудник
                //Структура сущности предмета
                DBItemInstance ChestInstance = new DBItemInstance
                {
                    GbId = 1612257704, // 
                    Affixes = "",
                    Attributes = "312,:1|1,401298E-45;101,:0|0;329,:1|1,401298E-45;48,:1073741824|2;51,:1073741824|2;52,:1073741824|2;53,:1073741824|2;326,:1|1,401298E-45;330,:1425612098|8,559502E+12"
                };
                // Добавляем сущность в базу
                DBSessions.AccountSession.SaveOrUpdate(ChestInstance);
                // Структура предмета
                DBInventory BARChest = new DBInventory
                {
                    DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID), // Привязка к используемому аккаунту
                    DBItemInstance = ChestInstance, // Использовать свеже созданную сущность предмета
                    //Hardcore = isHardcore, // Хардкорный или нет персонаж
                    DBToon = newDBToon, // Выдать созданному персонажу
                    EquipmentSlot = 2, // Вооружить в первую руку
                    LocationX = 0,
                    LocationY = 0
                };
                // Добавляем предмет в базу
                DBSessions.AccountSession.SaveOrUpdate(BARChest);
                #endregion

                #region Штаны
                //Структура сущности предмета
                DBItemInstance PantsInstance = new DBItemInstance
                {
                    GbId = -1512732138, // 
                    Affixes = "",
                    Attributes = "312,:0|0;101,:0|0;329,:1|1,401298E-45;48,:1084227584|5;51,:1084227584|5;52,:1084227584|5;53,:1084227584|5;326,:1|1,401298E-45;330,:56548088|6,54913E-37;315,:1|1,401298E-45"
                };
                // Добавляем сущность в базу
                DBSessions.AccountSession.SaveOrUpdate(PantsInstance);
                // Структура предмета
                DBInventory BARPants = new DBInventory
                {
                    DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID), // Привязка к используемому аккаунту
                    DBItemInstance = PantsInstance, // Использовать свеже созданную сущность предмета
                   // Hardcore = isHardcore, // Хардкорный или нет персонаж
                    DBToon = newDBToon, // Выдать созданному персонажу
                    EquipmentSlot = 9, // Вооружить в первую руку
                    LocationX = 0,
                    LocationY = 0
                };
                // Добавляем предмет в базу
                DBSessions.AccountSession.SaveOrUpdate(BARPants);
                #endregion
            }
            #endregion
            //*/
            #endregion

            #region Начало прогресса. 1 Акт, Пока не взятый квест.
            DBProgressToon StartProgress = new DBProgressToon
            {
                DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID), // Привязка к используемому аккаунту
                DBToon = newDBToon, // Выдать созданному персонажу
                MaximumQuest = 87700, // Максимальный квест
                MaximumAct = 0, // Максимальный квест
                StepOfQuest = 0, //Текущий шаг квеста
                ActiveAct = 0, // Активный акт
                ActiveQuest = -1, // Активный квест
                StepIDofQuest = 1,
                LoreCollected = "",
                StatusOfWings = 0,

                LastCheckPoint = -1,
                LastWorld = -1,

                WaypointStatus = 1,
                WaypointStatus2 = 1,
                WaypointStatus3 = 1,
                WaypointStatus4 = 1

            };
            DBSessions.AccountSession.SaveOrUpdate(StartProgress);
            DBPortalOfToon NullPortal = new DBPortalOfToon
            {
                DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID),
                DBToon = newDBToon,
                X = 0,
                Y = 0,
                Z = 0,
                WorldDest = 0
            };
            DBSessions.AccountSession.SaveOrUpdate(NullPortal);
            DBArtisansOfToon BaseArtisans = new DBArtisansOfToon
            {
                DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID),
                DBToon = newDBToon,
                Blacksmith = 1,
                Jeweler = 1,
                Mystic = 1,
            };
            DBSessions.AccountSession.SaveOrUpdate(BaseArtisans);
            DBHirelingsOfToon BaseInfoOfHirelings = new DBHirelingsOfToon
            {
                DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID),
                DBToon = newDBToon,

                Level_Templar = 1,
                Level_Scoundrel = 1,
                Level_Enchantress = 1,

                Experience_Templar = 1200,
                Experience_Scoundrel = 1200,
                Experience_Enchantress = 1200,

                Templar_Skill1 = -1,
                Templar_Skill2 = -1,
                Templar_Skill3 = -1,
                Templar_Skill4 = -1,

                Scoundrel_Skill1 = -1,
                Scoundrel_Skill2 = -1,
                Scoundrel_Skill3 = -1,
                Scoundrel_Skill4 = -1,

                Enchantress_Skill1 = -1,
                Enchantress_Skill2 = -1,
                Enchantress_Skill3 = -1,
                Enchantress_Skill4 = -1

            };
            DBSessions.AccountSession.SaveOrUpdate(BaseInfoOfHirelings);
            #endregion


            
            DBSessions.AccountSession.SaveOrUpdate(dbGameAccount);
            DBSessions.AccountSession.Flush();
            DBSessions.AccountSession.Refresh(dbGameAccount);

            return GetToonByLowID(newDBToon.Id);
        }

        public static void DeleteToon(Toon toon)
        {
            if (toon == null)
                return;

            //remove toonActiveSkills
            if (toon.DBToon.DBActiveSkills != null)
            {
                DBSessions.AccountSession.Delete(toon.DBToon.DBActiveSkills);
                toon.DBToon.DBActiveSkills = null;
            }

            //remove toon inventory
            var inventoryToDelete = DBSessions.AccountSession.Query<DBInventory>().Where(inv => inv.DBToon.Id == toon.DBToon.Id);
            foreach (var inv in inventoryToDelete)
            {
                //toon.DBToon.DBGameAccount.DBInventories.Remove(inv);
                DBSessions.AccountSession.Delete(inv);
            }




            //remove lastplayed hero if it was toon
            if (toon.DBToon.DBGameAccount.LastPlayedHero != null && toon.DBToon.DBGameAccount.LastPlayedHero.Id == toon.DBToon.Id)
                toon.DBToon.DBGameAccount.LastPlayedHero = null;


            //remove toon from dbgameaccount
            while (toon.DBToon.DBGameAccount.DBToons.Contains(toon.DBToon))
                toon.DBToon.DBGameAccount.DBToons.Remove(toon.DBToon);

            //save all this thinks
            DBSessions.AccountSession.SaveOrUpdate(toon.DBToon.DBGameAccount);
            DBSessions.AccountSession.Delete(toon.DBToon);
            DBSessions.AccountSession.Flush();


            //remove toon from loadedToon list
            if (LoadedToons.Contains(toon))
                LoadedToons.Remove(toon);

            Logger.Debug("Deleting toon {0}", toon.PersistentID);
        }


        public static void Sync()
        {
            foreach (var toon in LoadedToons)
            {
                SaveToDB(toon);
            }
        }

        public static void SaveToDB(Toon toon)
        {
            try
            {
                // save character base data
                var dbToon = DBSessions.AccountSession.Get<DBToon>(toon.PersistentID);
                dbToon.Name = toon.Name;
                /*dbToon.HashCode = toon.HashCode;*/
                dbToon.Class = toon.Class;
                dbToon.Flags = toon.Flags;
                dbToon.Level = toon.Level;
                dbToon.Experience = toon.ExperienceNext;
                dbToon.DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(toon.GameAccount.PersistentID);
                dbToon.TimePlayed = toon.TimePlayed;
                dbToon.Deleted = toon.Deleted;

                DBSessions.AccountSession.SaveOrUpdate(dbToon);
                DBSessions.AccountSession.Flush();
            }
            catch (Exception e)
            {
                Logger.ErrorException(e, "Toon.SaveToDB()");
            }
        }

    }
}
