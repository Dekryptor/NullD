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

using NullD.Common.Helpers.Hash;
using NullD.Common.Logging;
using NullD.Core.GS.Map;
using NullD.Core.GS.Players;
using NullD.Net.GS.Message.Definitions.Misc;
using NullD.Net.GS.Message.Definitions.World;
using NullD.Net.GS.Message.Fields;
using NullD.Net.GS.Message.Definitions.Map;
using NullD.Core.GS.Common.Types.TagMap;
using System.Collections.Generic;
using NullD.Net.GS.Message;
using NullD.Core.GS.Common.Types.Math;
using NullD.Common.Storage;
using NullD.Common.Storage.AccountDataBase.Entities;
using NullD.Net.GS.Message.Definitions.Pet;
using NullD.Core.LogNet.Toons;
using NullD.Core.GS.Actors.Implementations;

namespace NullD.Core.GS.Actors
{
    public class Portal : Actor
    {
        static readonly Logger Logger = LogManager.CreateLogger();

        public override ActorType ActorType { get { return ActorType.Gizmo; } }

        private ResolvedPortalDestination Destination { get; set; }
        private int MinimapIcon;

        public Portal(World world, int snoId, TagMap tags)
            : base(world, snoId, tags)
        {


            try
            {
                // Порталы на кладбище
                if (world.WorldSNO.Id == 71150 && this.ActorSNO.Id == 176002)
                {
                    #region Рандомим положение
                    var portals = world.GetActorsBySNO(176002);
                    int IdofPortal = 0;

                    foreach (var portal in portals)
                        if (portal.CurrentScene.SceneSNO.Name.Contains("Wilderness_MainGraveyard"))
                            if (portal.CurrentScene.SceneSNO.Id != IdofPortal)
                            {
                                IdofPortal = portal.CurrentScene.SceneSNO.Id;
                            }
                            else
                                portal.Destroy();

                    #endregion

                    if (portals.Count == 0)
                    {
                        this.Destination = new ResolvedPortalDestination
                        {
                            WorldSNO = 72636,//tags[MarkerKeys.DestinationWorld].Id,
                            DestLevelAreaSNO = tags[MarkerKeys.DestinationLevelArea].Id,
                            StartingPointActorTag = tags[MarkerKeys.DestinationActorTag]
                        };

                        // Override minimap icon in merkerset tags
                        if (tags.ContainsKey(MarkerKeys.MinimapTexture))
                        {
                            MinimapIcon = tags[MarkerKeys.MinimapTexture].Id;
                        }
                        else
                        {
                            MinimapIcon = ActorData.TagMap[ActorKeys.MinimapMarker].Id;
                        }
                    }
                    else if (portals.Count == 1)
                    {
                        this.Destination = new ResolvedPortalDestination
                        {
                            WorldSNO = 72637,//tags[MarkerKeys.DestinationWorld].Id,
                            DestLevelAreaSNO = tags[MarkerKeys.DestinationLevelArea].Id,
                            StartingPointActorTag = 172
                        };

                        // Override minimap icon in merkerset tags
                        if (tags.ContainsKey(MarkerKeys.MinimapTexture))
                        {
                            MinimapIcon = tags[MarkerKeys.MinimapTexture].Id;
                        }
                        else
                        {
                            MinimapIcon = ActorData.TagMap[ActorKeys.MinimapMarker].Id;
                        }
                    }
                    else if (portals.Count == 2)
                    {
                        this.Destination = new ResolvedPortalDestination
                        {
                            WorldSNO = 154587,//tags[MarkerKeys.DestinationWorld].Id,
                            DestLevelAreaSNO = tags[MarkerKeys.DestinationLevelArea].Id,
                            StartingPointActorTag = tags[MarkerKeys.DestinationActorTag]
                        };

                        // Override minimap icon in merkerset tags
                        if (tags.ContainsKey(MarkerKeys.MinimapTexture))
                        {
                            MinimapIcon = tags[MarkerKeys.MinimapTexture].Id;
                        }
                        else
                        {
                            MinimapIcon = ActorData.TagMap[ActorKeys.MinimapMarker].Id;
                        }
                    }
                    else if (portals.Count == 3)
                    {
                        this.Destination = new ResolvedPortalDestination
                        {
                            WorldSNO = 102299,//tags[MarkerKeys.DestinationWorld].Id,
                            DestLevelAreaSNO = tags[MarkerKeys.DestinationLevelArea].Id,
                            StartingPointActorTag = tags[MarkerKeys.DestinationActorTag]
                        };

                        // Override minimap icon in merkerset tags
                        if (tags.ContainsKey(MarkerKeys.MinimapTexture))
                        {
                            MinimapIcon = tags[MarkerKeys.MinimapTexture].Id;
                        }
                        else
                        {
                            MinimapIcon = ActorData.TagMap[ActorKeys.MinimapMarker].Id;
                        }
                    }
                    else
                    {
                        this.Destination = new ResolvedPortalDestination
                        {
                            WorldSNO = tags[MarkerKeys.DestinationWorld].Id,
                            DestLevelAreaSNO = tags[MarkerKeys.DestinationLevelArea].Id,
                            StartingPointActorTag = tags[MarkerKeys.DestinationActorTag]
                        };

                        // Override minimap icon in merkerset tags
                        if (tags.ContainsKey(MarkerKeys.MinimapTexture))
                        {
                            MinimapIcon = tags[MarkerKeys.MinimapTexture].Id;
                        }
                        else
                        {
                            MinimapIcon = ActorData.TagMap[ActorKeys.MinimapMarker].Id;
                        }
                    }
                    //Повторная прочистка от дублирующих порталов.
                    portals = world.GetActorsBySNO(176002);
                    IdofPortal = 0;
                    foreach (var portal in portals)
                        if (portal.CurrentScene.SceneSNO.Name.Contains("Wilderness_MainGraveyard"))
                            if (portal.CurrentScene.SceneSNO.Id != IdofPortal)
                            {
                                IdofPortal = portal.CurrentScene.SceneSNO.Id;
                            }
                            else
                                portal.Destroy();
                    if (portals.Count == 4)
                        (portals[3] as Portal).Destination = new ResolvedPortalDestination
                        {
                            WorldSNO = 72637,
                            DestLevelAreaSNO = 83265,
                            StartingPointActorTag = 172
                        };
                    IdofPortal = 0;
                }
                //Патч выхода из колодца
                else if (world.WorldSNO.Id == 161961 && this.ActorSNO.Id == 176537)
                {
                    this.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = 71150,
                        DestLevelAreaSNO = 91324,
                        StartingPointActorTag = 172
                    };

                    // Override minimap icon in merkerset tags
                    if (tags.ContainsKey(MarkerKeys.MinimapTexture))
                    {
                        MinimapIcon = tags[MarkerKeys.MinimapTexture].Id;
                    }
                    else
                    {
                        MinimapIcon = ActorData.TagMap[ActorKeys.MinimapMarker].Id;
                    }

                }
                else if (world.WorldSNO.Id == 166640 && this.ActorSNO.Id == 204901)
                {
                    this.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = 109513,//tags[MarkerKeys.DestinationWorld].Id,
                        DestLevelAreaSNO = 109514,
                        StartingPointActorTag = 172
                    };
                }
                //Стандартная генерация
                else
                {
                    this.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = tags[MarkerKeys.DestinationWorld].Id,
                        DestLevelAreaSNO = tags[MarkerKeys.DestinationLevelArea].Id,
                        StartingPointActorTag = tags[MarkerKeys.DestinationActorTag]
                    };

                    // Override minimap icon in merkerset tags
                    if (tags.ContainsKey(MarkerKeys.MinimapTexture))
                    {
                        MinimapIcon = tags[MarkerKeys.MinimapTexture].Id;
                    }
                    else
                    {
                        MinimapIcon = ActorData.TagMap[ActorKeys.MinimapMarker].Id;
                    }
                }

            }
            catch (KeyNotFoundException)
            {
                #region Портальная система
                if (this.ActorSNO.Id == 5648)
                {
                    //Generate Portal
                    foreach (var player in this.World.Players.Values)
                    {
                        var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Toon.PersistentID);
                        if (dbQuestProgress.ActiveAct == 0)
                        {
                            this.Destination = new ResolvedPortalDestination
                            {
                                WorldSNO = 71150,
                                DestLevelAreaSNO = 19947,
                                StartingPointActorTag = -100
                            };
                        }
                        else if (dbQuestProgress.ActiveAct == 100)
                        {
                            this.Destination = new ResolvedPortalDestination
                            {
                                WorldSNO = 161472,
                                DestLevelAreaSNO = 168314,
                                StartingPointActorTag = -100
                            };
                        }
                        if (dbQuestProgress.ActiveAct == 200)
                        {
                            this.Destination = new ResolvedPortalDestination
                            {
                                WorldSNO = 172909,
                                DestLevelAreaSNO = 92945,
                                StartingPointActorTag = -100
                            };
                        }
                        if (dbQuestProgress.ActiveAct == 300)
                        {
                            this.Destination = new ResolvedPortalDestination
                            {
                                WorldSNO = 178152,
                                DestLevelAreaSNO = 197101,
                                StartingPointActorTag = -100
                            };
                        }
                        DBSessions.AccountSession.Flush();
                    }
                    Logger.Warn("Portal to Home {0} created", this.ActorSNO.Id);

                }
                #endregion
                #region Спуск на второй уровень в подземелье на кладбище
                else if (world.WorldSNO.Id == 154587 && world.GetActorsBySNO(176002) != null)
                {
                    var Portal = world.GetActorBySNO(176002);
                    if (Portal != null)
                    {
                        this.Destination = new ResolvedPortalDestination
                        {
                            WorldSNO = 60600,
                            DestLevelAreaSNO = 60601,
                            StartingPointActorTag = 172 //30
                        };
                    }
                    else
                    {
                        int ScenetoDest = 0;
                        int LevelArea = 0;
                        int World = 0;
                        foreach (var Gamer in this.World.Game.Players.Values)
                        {
                            if (Gamer.PlayerIndex == 0)
                            {
                                World = Gamer.World.WorldSNO.Id;
                                ScenetoDest = Gamer.CurrentScene.SceneSNO.Id;
                                if (Gamer.CurrentScene.Specification.SNOLevelAreas[1] != -1)
                                    LevelArea = Gamer.CurrentScene.Specification.SNOLevelAreas[1];
                                else
                                    LevelArea = Gamer.CurrentScene.Specification.SNOLevelAreas[0];
                            }
                        }

                        this.Destination = new ResolvedPortalDestination
                        {
                            WorldSNO = World,
                            DestLevelAreaSNO = LevelArea,
                            StartingPointActorTag = ScenetoDest
                        };
                    }
                }
                else if (world.WorldSNO.Id == 60600)
                {
                    this.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = 154587,
                        DestLevelAreaSNO = 154588,
                        StartingPointActorTag = 171 //30
                    };
                }
                #endregion
               
                #region Умное вычисление выхода
                else
                {
                    //102231 - Пустыня
                    Logger.Warn("Портал - {0} Не определён до конца, исполнение функции ''умного'' вычисления для выхода.", this.ActorSNO.Id);
                    int ScenetoDest = 0;
                    int LevelArea = 0;
                    int World = 0;
                    foreach (var Gamer in this.World.Game.Players.Values)
                    {
                        if (Gamer.PlayerIndex == 0)
                        {
                            World = Gamer.World.WorldSNO.Id;
                            ScenetoDest = Gamer.CurrentScene.SceneSNO.Id;
                            if (Gamer.CurrentScene.Specification.SNOLevelAreas[1] != -1)
                                LevelArea = Gamer.CurrentScene.Specification.SNOLevelAreas[1];
                            else
                                LevelArea = Gamer.CurrentScene.Specification.SNOLevelAreas[0];
                        }
                    }

                    this.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = World,
                        DestLevelAreaSNO = LevelArea,
                        StartingPointActorTag = ScenetoDest
                    };
                }
                #endregion
            }
            this.Field2 = 16;

            // FIXME: Hardcoded crap; probably don't need to set most of these. /komiga
            //this.Attributes[GameAttribute.MinimapActive] = true;
            //this.Attributes[GameAttribute.Hitpoints_Max_Total] = 1f;
            //this.Attributes[GameAttribute.Hitpoints_Max] = 0.0009994507f;
            //this.Attributes[GameAttribute.Hitpoints_Total_From_Level] = 3.051758E-05f;
            //this.Attributes[GameAttribute.Hitpoints_Cur] = 0.0009994507f;
            //this.Attributes[GameAttribute.Level] = 1;

            // EREKOSE STUFF
            //Logger.Debug(" (Portal ctor) position is {0}", this._position);
            //Logger.Debug(" (Portal ctor) quest range is {0}", this._questRange);
            // Logger.Debug(" (Portal ctor) is in scene SNO {0}", this.CurrentScene.SceneSNO);            
            //Logger.Debug(" (Portal Ctor) portal used has actor SNO {3}, SNO Name {0}, exists in world sno {1}, has dest world sno {2}", this.ActorSNO.Name, tags[MarkerKeys.DestinationWorld].Id, tags[MarkerKeys.DestinationWorld].Id, snoId);

        }

        public override bool Reveal(Player player)
        {
            //Logger.Debug(" (Reveal) portal {0} has location {1}", this.ActorSNO, this._position);


            if (!base.Reveal(player) || this.Destination == null)
                return false;

            player.InGameClient.SendMessage(new PortalSpecifierMessage()
            {
                ActorID = this.DynamicID,
                Destination = this.Destination
            });

            // Show a minimap icon
            NullD.Common.MPQ.Asset asset;
            string markerName = "";

            if (NullD.Common.MPQ.MPQStorage.Data.Assets[Common.Types.SNO.SNOGroup.LevelArea].TryGetValue(this.Destination.DestLevelAreaSNO, out asset))
                markerName = System.IO.Path.GetFileNameWithoutExtension(asset.FileName);

            

            if (this.ActorSNO.Id != 5648 & this.ActorSNO.Id != 5660)// && this.ActorSNO.Id != 229013)
            {
                var MapMarker = this.World.SpawnMonsterWithGet(4686, new Vector3D(Position.X, Position.Y, Position.Z - 1));
                //uint Need = World.NewActorID - 2;

                var actors = this.GetActorsInRange(2f);
                
                if (this.ActorSNO.Id != 5648 && this.ActorSNO.Id != 229013)
                {
                    MapMarker.Attributes[GameAttribute.MinimapIconOverride] = this.ActorData.TagMap[ActorKeys.MinimapMarker].Id;
                    
                }
                else
                {
                    MapMarker.Attributes[GameAttribute.MinimapIconOverride] = 102321;
                }
                MapMarker.Attributes[GameAttribute.MinimapActive] = true;
                MapMarker.Attributes[GameAttribute.MinimapDisableArrow] = true;

                MapMarker.Attributes.BroadcastChangedIfRevealed();
                   
            }
            return true;
        }

        public override void OnTargeted(Player player, TargetMessage message)
        {
            Logger.Debug("(OnTargeted) Portal has been activated ");

            var world = this.World.Game.GetWorld(this.Destination.WorldSNO);

            if (world == null)
            {
                Logger.Warn("Portal's destination world does not exist (WorldSNO = {0})", this.Destination.WorldSNO);
                return;
            }

            #region Не санкционированные порталы)
            if (this.Destination.StartingPointActorTag == -100)
            {
                Vector3D ToPortal = new Vector3D(2985.6241f, 2795.627f, 24.04532f);
                Vector3D ToPortal2Act = new Vector3D(310.739f, 275.8123f, 0.09997072f);
                Vector3D ToPortal3Act = new Vector3D(390f, 402f, 0f);
                Vector3D ToPortal4Act = new Vector3D(390f, 402f, 0f);



                //Сохраняем в базу координаты для обратного портала.
                var dbPortalOfToon = DBSessions.AccountSession.Get<DBPortalOfToon>(player.Toon.PersistentID);
                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Toon.PersistentID);
                dbPortalOfToon.WorldDest = player.World.WorldSNO.Id;
                dbPortalOfToon.X = this.Position.X;
                dbPortalOfToon.Y = this.Position.Y;
                dbPortalOfToon.Z = this.Position.Z;
                DBSessions.AccountSession.SaveOrUpdate(dbPortalOfToon);

                Logger.Warn("Data for back portal Saved.");

                if (dbQuestProgress.ActiveAct == 0)
                {
                    var TristramHome = player.World.Game.GetWorld(71150);
                    var OldPortal = TristramHome.GetActorsBySNO(5648);
                    foreach (var OldP in OldPortal)
                    {
                        OldP.Destroy();
                    }

                    var ToHome = new Portal(player.World.Game.GetWorld(71150), 5648, player.World.Game.GetWorld(71150).StartingPoints[0].Tags);
                    ToHome.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = dbPortalOfToon.WorldDest,
                        DestLevelAreaSNO = player.CurrentScene.Specification.SNOLevelAreas[0],
                        StartingPointActorTag = -101
                    };
                    ToHome.EnterWorld(ToPortal);

                    if (player.World.Game.GetWorld(71150) != player.World)
                    {
                        /*
                        if (dbQuestProgress.ActiveQuest == 72221 & dbQuestProgress.StepOfQuest == 10 & player.PlayerIndex == 0)
                        {
                            player.World.Game.Quests.NotifyQuest(72221, DiIiS.Common.MPQ.FileFormats.QuestStepObjectiveType.EventReceived, -1);
                            dbQuestProgress.StepOfQuest = 11;
                        }*/
                        player.ChangeWorld(player.World.Game.GetWorld(71150), ToPortal);
                        if (dbQuestProgress.ActiveQuest == 72738 && dbQuestProgress.StepOfQuest == 18)
                        {
                            //player.World.Game.Quests.NotifyQuest(72738, DiIiS.Common.MPQ.FileFormats.QuestStepObjectiveType.EventReceived, -1);
                            dbQuestProgress.ActiveQuest = 73236;
                            dbQuestProgress.StepOfQuest = -1;
                        }
                        player.ChangeWorld(player.World.Game.GetWorld(71150), ToPortal);

                        if (player.ActiveHireling != null)
                        {
                            player.ActiveHireling.ChangeWorld(world, ToPortal);
                            player.InGameClient.SendMessage(new PetMessage()
                            {
                                Field0 = 0,
                                Field1 = 0,
                                PetId = player.ActiveHireling.DynamicID,
                                Field3 = 0,
                            });
                        }
                        
                    }
                    else
                    {
                        player.Teleport(ToPortal);
                        if (player.ActiveHireling != null)
                        {
                            player.ActiveHireling.Teleport(ToPortal);
                        }
                    }
                }
                else if (dbQuestProgress.ActiveAct == 100)
                {
                    var CaldeumHome = player.World.Game.GetWorld(161472);
                    var OldPortal = CaldeumHome.GetActorsBySNO(5648);
                    foreach (var OldP in OldPortal)
                    {
                        OldP.Destroy();
                    }

                    var ToHome = new Portal(player.World.Game.GetWorld(161472), 5648, player.World.Game.GetWorld(161472).StartingPoints[0].Tags);
                    ToHome.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = dbPortalOfToon.WorldDest,
                        DestLevelAreaSNO = player.CurrentScene.Specification.SNOLevelAreas[0],
                        StartingPointActorTag = -101
                    };

                    ToHome.EnterWorld(ToPortal2Act);

                    if (player.World.Game.GetWorld(161472) != player.World)
                    {
                        player.ChangeWorld(player.World.Game.GetWorld(161472), ToPortal2Act);
                        if (player.ActiveHireling != null)
                        {
                            player.ActiveHireling.ChangeWorld(world, ToPortal2Act);
                            player.InGameClient.SendMessage(new PetMessage()
                            {
                                Field0 = 0,
                                Field1 = 0,
                                PetId = player.ActiveHireling.DynamicID,
                                Field3 = 0,
                            });
                        }
                    }
                }
                else if (dbQuestProgress.ActiveAct == 200)
                {
                    var BastionHome = player.World.Game.GetWorld(172909);
                    var OldPortal = BastionHome.GetActorsBySNO(5648);
                    foreach (var OldP in OldPortal)
                    {
                        OldP.Destroy();
                    }

                    var ToHome = new Portal(player.World.Game.GetWorld(172909), 5648, player.World.Game.GetWorld(172909).StartingPoints[0].Tags);
                    ToHome.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = dbPortalOfToon.WorldDest,
                        DestLevelAreaSNO = player.CurrentScene.Specification.SNOLevelAreas[0],
                        StartingPointActorTag = -101
                    };
                    ToHome.EnterWorld(ToPortal3Act);

                    if (player.World.Game.GetWorld(172909) != player.World)
                        player.ChangeWorld(player.World.Game.GetWorld(172909), ToPortal3Act);
                    else
                        player.Teleport(ToPortal3Act);
                }
                else if (dbQuestProgress.ActiveAct == 300)
                {
                    var BastionHome = player.World.Game.GetWorld(178152);
                    var OldPortal = BastionHome.GetActorsBySNO(5648);
                    foreach (var OldP in OldPortal)
                    {
                        OldP.Destroy();
                    }

                    var ToHome = new Portal(player.World.Game.GetWorld(178152), 5648, player.World.Game.GetWorld(178152).StartingPoints[0].Tags);
                    ToHome.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = dbPortalOfToon.WorldDest,
                        DestLevelAreaSNO = player.CurrentScene.Specification.SNOLevelAreas[0],
                        StartingPointActorTag = -101
                    };
                    ToHome.EnterWorld(ToPortal4Act);

                    if (player.World.Game.GetWorld(178152) != player.World)
                        player.ChangeWorld(player.World.Game.GetWorld(178152), ToPortal4Act);
                    else
                        player.Teleport(ToPortal4Act);
                }

                DBSessions.AccountSession.Flush();
            }
            //Портал из Города
            else if (this.Destination.StartingPointActorTag == -101)
            {
                var dbPortalOfToon = DBSessions.AccountSession.Get<DBPortalOfToon>(player.Toon.PersistentID);
                Vector3D ToPortal = new Vector3D(dbPortalOfToon.X, dbPortalOfToon.Y, dbPortalOfToon.Z);
                var DestWorld = player.World.Game.GetWorld(dbPortalOfToon.WorldDest);
                var oldPortals = DestWorld.GetActorsBySNO(5648);

                foreach (var OldP in oldPortals)
                {
                    OldP.Destroy();
                }

                if (player.World.Game.GetWorld(dbPortalOfToon.WorldDest) != player.World)
                {
                    player.ChangeWorld(player.World.Game.GetWorld(dbPortalOfToon.WorldDest), ToPortal);
                    if (player.ActiveHireling != null)
                    {
                        player.ActiveHireling.ChangeWorld(world, ToPortal);
                        player.InGameClient.SendMessage(new PetMessage()
                        {
                            Field0 = 0,
                            Field1 = 0,
                            PetId = player.ActiveHireling.DynamicID,
                            Field3 = 0,
                        });
                    }
                }
                else
                {
                    player.Teleport(ToPortal);
                    if (player.ActiveHireling != null)
                    {
                        player.ActiveHireling.Teleport(ToPortal);
                        player.InGameClient.SendMessage(new PetMessage()
                        {
                            Field0 = 0,
                            Field1 = 0,
                            PetId = player.ActiveHireling.DynamicID,
                            Field3 = 0,
                        });
                    }
                }
            }
            #endregion

            var startingPoint = world.GetStartingPointById(this.Destination.StartingPointActorTag);

            if (startingPoint != null)
                player.ChangeWorld(world, startingPoint);
            else
            {
                #region Использование умного телепорта
                if (this.Destination.StartingPointActorTag != 0)
                {
                    StartingPoint NeededStartingPoint = world.GetStartingPointById(this.Destination.StartingPointActorTag);
                    var DestWorld = world.Game.GetWorld(this.Destination.WorldSNO);
                    var StartingPoints = DestWorld.GetActorsBySNO(5502);
                    foreach (var ST in StartingPoints)
                    {
                        if (ST.CurrentScene.SceneSNO.Id == this.Destination.StartingPointActorTag)
                            NeededStartingPoint = (ST as StartingPoint);
                    }
                    player.ChangeWorld(DestWorld, NeededStartingPoint);
                }
                #endregion
                else
                {
                    Logger.Warn("Portal's tagged starting point does not exist (Tag = {0})", this.Destination.StartingPointActorTag);
                }
            }
                
        }
    }
}