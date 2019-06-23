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
using System.Text;
using NullD.Common.MPQ.FileFormats;
using NullD.Net.GS.Message.Definitions.ACD;
using NullD.Net.GS.Message.Definitions.Animation;
using NullD.Core.GS.Common.Types.Math;
using NullD.Core.GS.Generators;
using NullD.Common.Logging;
using System.Threading.Tasks;
using System.Threading;
using NullD.Core.GS.AI.Brains;
using NullD.Core.GS.Actors.Implementations.Hirelings;
using NullD.Net.GS.Message;
using NullD.Common.Storage.AccountDataBase.Entities;
using NullD.Common.Storage;

namespace NullD.Core.GS.QuestEvents.Implementations
{
    class _198541 : QuestEvent
    {

        private static readonly Logger Logger = LogManager.CreateLogger();


        public _198541()
             : base(198541)
        {
        }
        private Boolean HadConversation = true;
        private Players.Player MasterPlayer;
        private static ThreadLocal<Random> _threadRand = new ThreadLocal<Random>(() => new Random());
        public static Random Rand { get { return _threadRand.Value; } }
        List<Vector3D> monstersAlive = new List<Vector3D> { };

        public override void Execute(Map.World world)
        {
            // Проверка на ненужную Лию
            if (world.HasActor(72))
            {
                world.GetActorByDynamicId(72).Destroy();
            }

            //Берем нужную Лию =)
            var LeahBrains = world.GetActorByDynamicId(83);

            //Берем путевую точку Нового Тристрама
            var NewTristramPortal = world.GetActorBySNO(223757);

            if (HadConversation)
            {
                HadConversation = false;
                Logger.Debug(" RESCUE CAIN QUEST STARTED ");
                world.Game.Quests.NotifyQuest(72095, QuestStepObjectiveType.EventReceived, -1);
            }

            #region Создаем Лею как подругу.
            Hireling LeahFriend = new LeahParty(world, LeahBrains.ActorSNO.Id, LeahBrains.Tags);
            LeahFriend.Brain = new HirelingBrain(LeahFriend);
            foreach (var player in world.Players)
            {
                if (player.Value.PlayerIndex == 0)
                {
                    LeahFriend.GBHandle.Type = 4;
                    LeahFriend.GBHandle.GBID = 717705071;
                    LeahFriend.Attributes[GameAttribute.Pet_Creator] = player.Value.PlayerIndex;
                    LeahFriend.Attributes[GameAttribute.Pet_Type] = 0x8;
                    LeahFriend.Attributes[GameAttribute.Hitpoints_Max] = 100f;
                    LeahFriend.Attributes[GameAttribute.Hitpoints_Cur] = 80f;
                    LeahFriend.Attributes[GameAttribute.Attacks_Per_Second] = 1.6f;
                    LeahFriend.Attributes[GameAttribute.Pet_Owner] = player.Value.PlayerIndex;
                    LeahFriend.Attributes[GameAttribute.Untargetable] = false;
                    LeahFriend.Position = RandomDirection(player.Value.Position, 3f, 8f);
                    LeahFriend.RotationW = LeahBrains.RotationW;
                    LeahFriend.RotationAxis = LeahBrains.RotationAxis;
                    LeahFriend.EnterWorld(RandomDirection(player.Value.Position, 3f, 8f));
                    LeahFriend.Attributes[GameAttribute.Level]++;
                    player.Value.ActiveHireling = LeahFriend;
                    LeahFriend.Brain.Activate();
                    MasterPlayer = player.Value;
                }
                player.Value.Toon.ActiveQuest = 72095;
                player.Value.Toon.StepOfQuest = 1;
                player.Value.Toon.StepIDofQuest = 7;
            }
            #endregion
            // Убираем Лею NPC
            try { world.Leave(LeahBrains); }
            catch { }

            NewTristramPortal.Attributes[Net.GS.Message.GameAttribute.Gizmo_Has_Been_Operated] = false;
            var ListenerUsePortalTask = Task<bool>.Factory.StartNew(() => OnUseTeleporterListener(NewTristramPortal.DynamicID, world));
            ListenerUsePortalTask.ContinueWith(delegate //Ждём использования телепорта:
            {
                Logger.Debug(" Waypoint_NewTristram Objective done "); // Waypoint_NewTristram
            });

            var ListenerEnterToOldTristram = Task<bool>.Factory.StartNew(() => OnListenerToEnter(MasterPlayer, world));
            ListenerEnterToOldTristram.ContinueWith(delegate //Once killed:
            {
                Logger.Debug("Enter to Road Objective done ");
                var ListenerEnterToAdriaEnter = Task<bool>.Factory.StartNew(() => OnListenerToAndriaEnter(MasterPlayer, world));
                ListenerEnterToAdriaEnter.ContinueWith(delegate //Once killed:
                {
                    Logger.Debug("Enter to Adria Objective done ");
                });
            });
        }

        //just for the use of the portal
        private bool OnUseTeleporterListener(uint actorDynID, Map.World world)
        {
            if (world.HasActor(actorDynID))
            {
                try
                {
                    while (true)
                    {
                        var actor = world.GetActorByDynamicId(actorDynID); // it is not null :p

                        if (actor.Attributes[Net.GS.Message.GameAttribute.Gizmo_Has_Been_Operated])
                        {
                            world.Game.Quests.NotifyQuest(72095, QuestStepObjectiveType.InteractWithActor, -1);
                            foreach (var player in world.Players.Values)
                            {
                                player.Toon.ActiveQuest = 72095;
                                player.Toon.StepOfQuest = 2;
                            }
                            break;
                        }
                    }
                }
                catch { }
            }
            return true;
        }

        private bool OnListenerToEnter(Players.Player player, Map.World world)
        {
            while (true)
            {
                try
                {
                    int sceneID = player.CurrentScene.SceneSNO.Id;
                    if (sceneID == 90196)
                    {
                        foreach (var playerN in world.Players.Values)
                        {
                            playerN.Toon.ActiveQuest = 72095;
                            playerN.Toon.StepOfQuest = 3;
                        }

                        try
                        {
                            if (player.ActiveHireling != null)
                            {
                                Vector3D NearDoor = new Vector3D(1935.697f, 2792.971f, 40.23627f);
                                var facingAngle = Actors.Movement.MovementHelpers.GetFacingAngle(player.ActiveHireling.Position, NearDoor);
                                player.ActiveHireling.Brain.DeActivate();
                                player.ActiveHireling.Move(NearDoor, facingAngle);

                                StartConversation(world, 166678);
                            }
                        }

                        catch { }
                        break;
                    }
                }
                catch { Logger.Debug("Приостановка скрипта, идёт загрузка."); }
            }
            return true;
        }
        private bool OnListenerToAndriaEnter(Core.GS.Players.Player player, Core.GS.Map.World world)
        {
            while (true)
            {
                try
                {
                    if (player.World.WorldSNO.Id == 71150)
                    {
                        int sceneID = player.CurrentScene.SceneSNO.Id;
                        if (sceneID == 90293)
                        {
                            foreach (var playerN in world.Players.Values)
                            {
                                playerN.Toon.ActiveQuest = 72095;
                                playerN.Toon.StepOfQuest = 5;
                            }
                            world.Game.Quests.NotifyQuest(72095, QuestStepObjectiveType.EnterTrigger, -1);
                            break;
                        }
                    }
                }
                catch { }
            }

            return true;
        }
        public Vector3D RandomDirection(Vector3D position, float minRadius, float maxRadius)
        {
            float angle = (float)(Rand.NextDouble() * Math.PI * 2);
            float radius = minRadius + (float)Rand.NextDouble() * (maxRadius - minRadius);
            return new Vector3D(position.X + (float)Math.Cos(angle) * radius,
                                position.Y + (float)Math.Sin(angle) * radius,
                                position.Z);
        }

        private bool StartConversation(Map.World world, Int32 conversationId)
        {
            foreach (var player in world.Players)
            {
                player.Value.Conversations.StartConversation(conversationId);
            }
            return true;
        }
    }
}