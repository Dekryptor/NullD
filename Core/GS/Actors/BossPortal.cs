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
using NullD.Common.Storage;
using NullD.Common.Storage.AccountDataBase.Entities;
using NullD.Core.GS.Common.Types.Math;
using System.Threading.Tasks;
using NullD.Core.GS.Generators;
using NullD.Core.GS.Ticker;
using NullD.Core.GS.Common.Types.SNO;
using System.Windows;
using NullD.Core.GS.Actors.Implementations.Hirelings;
using NullD.Net.GS.Message;
using NullD.Core.GS.Actors.Implementations;
using NullD.Net.GS.Message.Definitions.Encounter;

namespace NullD.Core.GS.Actors
{
    public class BossPortal : Actor
    {
        static readonly Logger Logger = LogManager.CreateLogger();

        public override ActorType ActorType { get { return ActorType.Gizmo; } }
        private ResolvedPortalDestination Destination { get; set; }
        private BossEncounterMessage BossMessage { get; set; }
        public NullD.Common.MPQ.FileFormats.Scene.NavZoneDef NavZone { get; private set; }
        private int MinimapIcon;


        public BossPortal(World world, int snoId, TagMap tags)
            : base(world, snoId, tags)
        {
            try
            {

                int DestArea = 0;
                if ((tags[MarkerKeys.BossEncounter].Target as NullD.Common.MPQ.FileFormats.BossEncounter).Worlds[1] == 60713)
                    DestArea = 60714;
                else if ((tags[MarkerKeys.BossEncounter].Target as NullD.Common.MPQ.FileFormats.BossEncounter).Worlds[1] == 73261)
                    DestArea = 19789;
                else if ((tags[MarkerKeys.BossEncounter].Target as NullD.Common.MPQ.FileFormats.BossEncounter).Worlds[1] == 109143)
                    DestArea = 109149;
                else if ((tags[MarkerKeys.BossEncounter].Target as NullD.Common.MPQ.FileFormats.BossEncounter).Worlds[1] == 182976)
                { DestArea = 62726; this.Scale = 0.25f; }
                else if (this.ActorSNO.Id == 159580)
                    DestArea = 58494;
                else if ((tags[MarkerKeys.BossEncounter].Target as NullD.Common.MPQ.FileFormats.BossEncounter).Worlds[1] == 174449)
                    DestArea = 130163;
                else if ((tags[MarkerKeys.BossEncounter].Target as NullD.Common.MPQ.FileFormats.BossEncounter).Worlds[1] == 103910) //Кристальная коллонада
                    DestArea = 119882;
                else if ((tags[MarkerKeys.BossEncounter].Target as NullD.Common.MPQ.FileFormats.BossEncounter).Worlds[1] == 214956) //
                    DestArea = 215396;
                else if ((tags[MarkerKeys.BossEncounter].Target as NullD.Common.MPQ.FileFormats.BossEncounter).Worlds[1] == 166640) //[166640] a4Dun_LibraryOfFate
                    DestArea = 143648;
                else if ((tags[MarkerKeys.BossEncounter].Target as NullD.Common.MPQ.FileFormats.BossEncounter).Worlds[1] == 195200) //{[World] SNOId: 195200 DynamicId: 117440516 Name: caOut_Cellar_Alcarnus_Main}
                    DestArea = 195268;
                else if ((tags[MarkerKeys.BossEncounter].Target as NullD.Common.MPQ.FileFormats.BossEncounter).Worlds[1] == 78839)
                    DestArea = 90881;
                else if ((tags[MarkerKeys.BossEncounter].Target as NullD.Common.MPQ.FileFormats.BossEncounter).Worlds[1] == 109561) //Кристальная арка
                    DestArea = 109563;
                //62726
                if (DestArea == 62726)
                    this.Scale = 0.75f;
                else
                    this.Scale = 1;

                //[195199] [Scene] caOut_Arena_Alcarnus
                this.BossMessage = new BossEncounterMessage
                {
                    Field0 = 0,
                    snoEncounter = tags[MarkerKeys.BossEncounter].Id
                };

                this.Destination = new ResolvedPortalDestination
                {
                    WorldSNO = (tags[MarkerKeys.BossEncounter].Target as NullD.Common.MPQ.FileFormats.BossEncounter).Worlds[1],
                    DestLevelAreaSNO = DestArea,
                    StartingPointActorTag = (tags[MarkerKeys.BossEncounter].Target as NullD.Common.MPQ.FileFormats.BossEncounter).I9
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
            catch (KeyNotFoundException)
            {
                Logger.Warn("Boss Portal {0} has incomplete implementation", this.ActorSNO.Id);
            }
            this.Field2 = 16;
        }



        private bool StartConversation(Core.GS.Map.World world, System.Int32 conversationId)
        {
            foreach (var player in world.Players)
            {
                player.Value.Conversations.StartConversation(conversationId);
            }
            return true;
        }
        public static bool setActorOperable(Map.World world, System.Int32 snoId, bool status)
        {
            var actor = world.GetActorBySNO(snoId);
            foreach (var player in world.Players)
            {
                actor.Attributes[Net.GS.Message.GameAttribute.Gizmo_Has_Been_Operated] = status;

            }
            return true;
        }
        #region Условия
        private bool OnKillListenerCain(List<uint> monstersAlive, Core.GS.Map.World world)
        {
            System.Int32 monstersKilled = 0;
            var monsterCount = monstersAlive.Count; //Since we are removing values while iterating, this is set at the first real read of the mob counting.
            while (monstersKilled != monsterCount)
            {
                //Iterate through monstersAlive List, if found dead we start to remove em till all of em are dead and removed.
                for (int i = monstersAlive.Count - 1; i >= 0; i--)
                {
                    if (world.HasMonster(monstersAlive[i]))
                    {
                        //Alive: Nothing.
                    }
                    else
                    {
                        //If dead we remove it from the list and keep iterating.
                        Logger.Debug(monstersAlive[i] + " has been killed");
                        monstersAlive.RemoveAt(i);
                        monstersKilled++;
                    }
                }
            }
            return true;
        }
        private bool OnKillKingSkeletonsListener(List<uint> monstersAlive, Map.World world)
        {
            System.Int32 monstersKilled = 0;
            var monsterCount = monstersAlive.Count; //Since we are removing values while iterating, this is set at the first real read of the mob counting.
            while (monstersKilled != monsterCount)
            {
                //Iterate through monstersAlive List, if found dead we start to remove em till all of em are dead and removed.
                for (int i = monstersAlive.Count - 1; i >= 0; i--)
                {
                    if (world.HasMonster(monstersAlive[i]))
                    {
                        //Alive: Nothing.
                    }
                    else
                    {
                        //If dead we remove it from the list and keep iterating.
                        Logger.Debug(monstersAlive[i] + " has been killed");
                        monstersAlive.RemoveAt(i);
                        monstersKilled++;
                    }
                }
            }
            return true;
        }
        private bool OnKillButcherListener(List<uint> monstersAlive, Map.World world)
        {
            System.Int32 monstersKilled = 0;
            //bool MidMidle_Active = false;
            var monsterCount = monstersAlive.Count; //Since we are removing values while iterating, this is set at the first real read of the mob counting.
            while (monstersKilled != monsterCount)
            {

                /*
                if (MidMidle_Active == false)
                {
                    MidMidle_Active = true;

                    var Panel_MidMiddle_Base = world.GetActorBySNO(201426);

                    if (Panel_MidMiddle_Base == null)
                    {
                        world.SpawnMonster(201426, new Vector3D(120.9595f, 121.6244f, -0.1068707f));
                        Panel_MidMiddle_Base = world.GetActorBySNO(201426);
                    }

                    TickTimer Timeout1 = new SecondsTickTimer(world.Game, 2f);
                    var TimeoutToReady = System.Threading.Tasks.Task<bool>.Factory.StartNew(() => WaitToSpawn(Timeout1));
                    TimeoutToReady.ContinueWith(delegate
                    {
                        world.SpawnMonster(201428, Panel_MidMiddle_Base.Position);
                        var Panel_MidMiddle_Ready = world.GetActorBySNO(201428);

                        TickTimer Timeout2 = new SecondsTickTimer(world.Game, 4f);
                        var TimeoutToActive = System.Threading.Tasks.Task<bool>.Factory.StartNew(() => WaitToSpawn(Timeout2));
                        TimeoutToActive.ContinueWith(delegate
                        {
                            Panel_MidMiddle_Ready.Destroy();
                            world.SpawnMonster(201430, Panel_MidMiddle_Base.Position);
                            var Panel_MidMiddle_Active = world.GetActorBySNO(201430);

                            TickTimer Timeout3 = new SecondsTickTimer(world.Game, 5f);
                            var TimeoutToOff = System.Threading.Tasks.Task<bool>.Factory.StartNew(() => WaitToSpawn(Timeout3));
                            TimeoutToOff.ContinueWith(delegate
                            {
                                Panel_MidMiddle_Active.Destroy();
                                MidMidle_Active = false;
                            });
                        });
                    });
                }
                */
                //Iterate through monstersAlive List, if found dead we start to remove em till all of em are dead and removed.
                for (int i = monstersAlive.Count - 1; i >= 0; i--)
                {
                    if (world.HasMonster(monstersAlive[i]))
                    {
                        //Alive: Nothing.
                    }
                    else
                    {
                        //If dead we remove it from the list and keep iterating.
                        Logger.Debug(monstersAlive[i] + " has been killed");
                        monstersAlive.RemoveAt(i);
                        monstersKilled++;
                    }
                }
            }
            return true;
        }
        #endregion
        public override bool Reveal(Player player)
        {
            Logger.Debug(" (Reveal) portal {0} has location {1}", this.ActorSNO, this._position);


            if (!base.Reveal(player) || this.Destination == null)
                return false;

            player.InGameClient.SendMessage(new PortalSpecifierMessage()
            {
                ActorID = this.DynamicID,
                Destination = this.Destination
            });
            /*player.InGameClient.SendMessage(new BossEncounterMessage()
            {
                Field0 = 0,
                snoEncounter = this.BossMessage.snoEncounter
            });*/
            Attributes[Net.GS.Message.GameAttribute.Gizmo_Has_Been_Operated] = false;
            this.Attributes[Net.GS.Message.GameAttribute.Operatable] = true;
            this.Attributes[Net.GS.Message.GameAttribute.Gizmo_State] = 0;

            // Show a minimap icon
            this.World.SpawnMonster(4686, new Vector3D(Position.X, Position.Y, Position.Z - 1));
            var actors = this.GetActorsInRange(1f);
            foreach (var actor in actors)
            {
                if (actor.ActorSNO.Id == 4686)
                {
                    actor.Attributes[GameAttribute.MinimapIconOverride] = this.ActorData.TagMap[ActorKeys.MinimapMarker].Id;
                    actor.Attributes[GameAttribute.MinimapActive] = true;
                    actor.Attributes.BroadcastChangedIfRevealed();
                }
            }

            return true;
        }

        public override void OnTargeted(Player player, TargetMessage message)
        {
            Logger.Debug("(OnTargeted) Boss Portal has been activated ");

            var world = this.World.Game.GetWorld(this.Destination.WorldSNO);
            var now_world = player.World;

            player.CurrentBossPortal = this.BossMessage.snoEncounter;



            #region События
            if (this.Destination.WorldSNO == 60713)
            {
                //Enter to Leoric Hall
                if (player.PlayerIndex == 0)
                {
                    var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Toon.PersistentID);
                    if (dbQuestProgress.StepOfQuest > 12 || dbQuestProgress.ActiveQuest != 72095)
                    {
                        var Bridge = world.GetActorBySNO(5723);
                        world.BroadcastIfRevealed(new Net.GS.Message.Definitions.Animation.SetIdleAnimationMessage
                        {
                            ActorID = Bridge.DynamicID,
                            AnimationSNO = AnimationSetKeys.Open.ID

                        }, Bridge);
                        var minions = world.GetActorsBySNO(80652);
                        foreach (var minion in minions)
                        {
                            minion.Destroy();
                        }
                    }

                    DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                    DBSessions.AccountSession.Flush();
                }
            }
            else if (this.Destination.WorldSNO == 73261)
            {
                if (player.PlayerIndex == 0)
                {
                    var BossWorld = player.World.Game.GetWorld(73261);
                    Vector3D Point = new Vector3D(338.9958f, 468.3622f, -3.859601f);
                    //world.Game.Quests.Advance(72061);
                    //player.ChangeWorld(BossWorld, Point);
                    var AllSpawnPoint = world.GetActorsBySNO(5913);
                    Vector3D FistPoint = new Vector3D(291.9193f, 428.6796f, 0.1f);
                    Vector3D SecondPoint = new Vector3D(270.9105f, 426.223f, 0.1000026f);
                    Vector3D ThirdPoint = new Vector3D(241.2828f, 425.616f, 0.1f);
                    Vector3D FourPoint = new Vector3D(241.2051f, 435.0545f, 0.1f);

                    var SkeletonKing_Bridge = BossWorld.GetActorBySNO(461);

                    BossWorld.BroadcastIfRevealed(new NullD.Net.GS.Message.Definitions.Animation.SetIdleAnimationMessage
                    {
                        ActorID = SkeletonKing_Bridge.DynamicID,
                        AnimationSNO = Core.GS.Common.Types.TagMap.AnimationSetKeys.Open.ID,
                    }, SkeletonKing_Bridge);
                    // 461 -trDun_SkeletonKing_Bridge_Active

                    BossWorld.SpawnMonster(87012, FistPoint);
                    BossWorld.SpawnMonster(87012, SecondPoint);
                    BossWorld.SpawnMonster(87012, ThirdPoint);
                    BossWorld.SpawnMonster(87012, FourPoint);
                    var AllSkeletons = BossWorld.GetActorsBySNO(87012);
                    List<uint> SkeletonsList = new List<uint> { };
                    foreach (var Skelet in AllSkeletons)
                    {
                        SkeletonsList.Add(Skelet.DynamicID);
                    }
                    var ListenerKingSkeletons = Task<bool>.Factory.StartNew(() => OnKillKingSkeletonsListener(SkeletonsList, BossWorld));
                    //Ждём пока убьют
                    ListenerKingSkeletons.ContinueWith(delegate
                    {
                        world.Game.Quests.Advance(72061);
                        //5765 - Gate
                        var SkeletonGate = BossWorld.GetActorBySNO(5765);
                        var WalkableSkeletonGate = new Door(BossWorld, 5765, world.GetActorBySNO(5765).Tags);
                        WalkableSkeletonGate.Field2 = 16;
                        WalkableSkeletonGate.RotationAxis = world.GetActorBySNO(5765).RotationAxis;
                        WalkableSkeletonGate.RotationW = world.GetActorBySNO(5765).RotationW;
                        WalkableSkeletonGate.Attributes[GameAttribute.Gizmo_Has_Been_Operated] = true;
                        //NoDownGate.Attributes[GameAttribute.Gizmo_Operator_ACDID] = unchecked((int)player.DynamicID);
                        WalkableSkeletonGate.Attributes[GameAttribute.Gizmo_State] = 1;
                        WalkableSkeletonGate.Attributes[GameAttribute.Untargetable] = true;
                        WalkableSkeletonGate.Attributes.BroadcastChangedIfRevealed();
                        WalkableSkeletonGate.EnterWorld(world.GetActorBySNO(5765).Position);
                        SkeletonGate.Destroy();

                        BossWorld.BroadcastIfRevealed(new NullD.Net.GS.Message.Definitions.Animation.PlayAnimationMessage
                        {
                            ActorID = WalkableSkeletonGate.DynamicID,
                            Field1 = 5,
                            Field2 = 0,
                            tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                                {
                            new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                            {
                                Duration = 100,
                                AnimationSNO = WalkableSkeletonGate.AnimationSet.TagMapAnimDefault[Core.GS.Common.Types.TagMap.AnimationSetKeys.Opening],
                                PermutationIndex = 0,
                                Speed = 0.5f
                            }
                                }
                        }, WalkableSkeletonGate);
                        BossWorld.BroadcastIfRevealed(new NullD.Net.GS.Message.Definitions.Animation.SetIdleAnimationMessage
                        {
                            ActorID = WalkableSkeletonGate.DynamicID,
                            AnimationSNO = Core.GS.Common.Types.TagMap.AnimationSetKeys.Open.ID,
                        }, WalkableSkeletonGate);

                    });
                }
            }
            else if (this.Destination.WorldSNO == 182976)
            {
                //Покои королевы
                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Toon.PersistentID);
                if (dbQuestProgress.StepOfQuest == 3)
                {
                    dbQuestProgress.StepOfQuest = 4;
                }
                if (dbQuestProgress.StepOfQuest > 7)
                {
                    try
                    {
                        var UsedWeb = world.GetActorBySNO(104545);
                        UsedWeb.Destroy();
                    }
                    catch { }
                }
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();
            }
            else if (this.Destination.WorldSNO == 78839)
            {
                //var startingPoint = new Vector3D(0f, 0f, 0f);
                var startingPoint = new Vector3D(143.3902f, 143.1758f, 0.09997044f);
                var ButcherLair = world.Game.GetWorld(78839);
                ButcherLair.SpawnMonster(3526, new Vector3D(92.82627f, 90.92698f, 0.09997056f));

                //player.ChangeWorld(world, startingPoint);

                StartConversation(ButcherLair, 211980);
                List<uint> ButcherList = new List<uint> { };
                //Отключить дверь
                //ID двери - 105361
                var BlockDoor = ButcherLair.GetActorBySNO(105361);
                BlockDoor.Attributes[GameAttribute.Gizmo_Has_Been_Operated] = true;
                BlockDoor.Attributes[GameAttribute.Gizmo_Operator_ACDID] = unchecked((int)player.DynamicID);
                BlockDoor.Attributes[GameAttribute.Gizmo_State] = 1;
                BlockDoor.Attributes[GameAttribute.Untargetable] = true;
                BlockDoor.Attributes.BroadcastChangedIfRevealed();

                ButcherList.Add(ButcherLair.GetActorBySNO(3526).DynamicID);
                var ListenerButcher = Task<bool>.Factory.StartNew(() => OnKillButcherListener(ButcherList, ButcherLair));
                //Ждём пока убьют
                ListenerButcher.ContinueWith(delegate
                {
                    ButcherLair.Game.Quests.Advance(72801);
                    //Включить дверь

                    var OpenDoor = new Door(BlockDoor.World, BlockDoor.ActorSNO.Id, BlockDoor.Tags);
                    OpenDoor.Field2 = 0;
                    OpenDoor.RotationAxis = BlockDoor.RotationAxis;
                    OpenDoor.RotationW = BlockDoor.RotationW;
                    OpenDoor.Attributes.BroadcastChangedIfRevealed();
                    OpenDoor.EnterWorld(BlockDoor.Position);
                    BlockDoor.Destroy();
                });
            }

            else if (this.Destination.WorldSNO == 174449)
            {
                //Странник замена первого на второго
                //[181654] [Actor] Stranger_event_readScroll
                //[183117] [Actor] Stranger_Ritual
                var FalseStranger = world.GetActorBySNO(181654);
                var StrangerRitual = world.SpawnMonsterWithGet(183117, FalseStranger.Position);
                StrangerRitual.RotationAxis = FalseStranger.RotationAxis;
                StrangerRitual.RotationW = FalseStranger.RotationW;
                world.Leave(FalseStranger);
            }
            #endregion

            if (world == null)
            {
                Logger.Warn("Portal's destination world does not exist (WorldSNO = {0})", this.Destination.WorldSNO);
                return;
            }
            else
            {
                foreach (var plr in world.Game.Players.Values)
                {
                    plr.InGameClient.SendMessage(new BossEncounterMessage()
                    {
                        Field0 = player.PlayerIndex,
                        snoEncounter = this.BossMessage.snoEncounter,
                        ToWorldID = world.WorldSNO.Id
                    });
                }
            }
        }
    }
}