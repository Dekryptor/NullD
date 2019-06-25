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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using NullD.Common.Logging;
using NullD.Core.GS.Objects;
using NullD.Core.GS.Generators;
using NullD.Core.GS.Map;
using NullD.Core.GS.Players;
using NullD.Net.GS;
using NullD.Net.GS.Message;
using NullD.Net.GS.Message.Definitions.Game;
using NullD.Net.GS.Message.Definitions.Misc;
using NullD.Net.GS.Message.Definitions.Player;
using NullD.Net.GS.Message.Fields;
using NullD.Core.GS.Actors.Implementations.Hirelings;
using System.Threading.Tasks;
using NullD.Core.GS.Common.Types.Math;
using System.Collections.Generic;
using NullD.Net.LogNet;
using NullD.Common.Storage;
using NullD.Common.Storage.AccountDataBase.Entities;
using NullD.Core.GS.AI.Brains;
using NullD.Core.GS.Actors;

namespace NullD.Core.GS.Games
{
    public class Game : IMessageConsumer
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        /// <summary>
        /// The game id.
        /// </summary>
        public int GameId { get; private set; }

        /// <summary>
        /// Dictionary that maps gameclient's to players.
        /// </summary>
        public ConcurrentDictionary<GameClient, Player> Players { get; private set; }

        /// <summary>
        /// Dictionary that tracks objects and maps them to dynamicId's.
        /// </summary>
        private readonly ConcurrentDictionary<uint, DynamicObject> _objects;

        /// <summary>
        /// Dictionary that tracks world.
        /// NOTE: This tracks by WorldSNO rather than by DynamicID; this.Objects _does_ still contain the world since it is a DynamicObject
        /// </summary>
        private readonly ConcurrentDictionary<int, World> _worlds;

        /// <summary>
        /// Starting world's sno id.
        /// </summary>
        public int StartingWorldSNOId { get; private set; }

        /// <summary>
        /// Starting world for the game.
        /// </summary>
        public World StartingWorld { get { return GetWorld(this.StartingWorldSNOId); } }

        /// <summary>
        /// Player index counter.
        /// </summary>
        public int PlayerIndexCounter = -1;

        /// <summary>
        /// Update frequency for the game - 100 ms.
        /// </summary>
        public readonly long UpdateFrequency = 100;

        /// <summary>
        /// Incremented tick value on each Game.Update().
        /// </summary>
        public readonly int TickRate = 6;

        /// <summary>
        /// Tick counter.
        /// </summary>
        private int _tickCounter;

        /// <summary>
        /// Returns the latest tick count.
        /// </summary>
        public int TickCounter
        {
            get { return _tickCounter; }
        }

        /// <summary>
        /// Stopwatch that measures time takent to get a full Game.Update(). 
        /// </summary>
        private readonly Stopwatch _tickWatch;

        /// <summary>
        /// DynamicId counter for objects.
        /// </summary>
        private uint _lastObjectID = 0x00000001;

        /// <summary>
        /// Returns a new dynamicId for objects.
        /// </summary>
        public uint NewObjectID { get { return _lastObjectID++; } }

        /// <summary>
        /// DynamicId counter for scene.
        /// </summary>
        private uint _lastSceneID = 0x04000000;

        /// <summary>
        /// Returns a new dynamicId for scenes.
        /// </summary>
        public uint NewSceneID { get { return _lastSceneID++; } }

        /// <summary>
        /// DynamicId counter for worlds.
        /// </summary>
        private uint _lastWorldID = 0x07000000;

        /// <summary>
        /// Returns a new dynamicId for worlds.
        /// </summary>
        public uint NewWorldID { get { return _lastWorldID++; } }

        public QuestManager Quests { get; private set; }
        public AI.Pather Pathfinder { get; private set; }

        /// <summary>
        /// Creates a new game with given gameId.
        /// </summary>
        /// <param name="gameId"></param>
        public Game(int gameId, List<LogNetClient> clients)
        {
            this.GameId = gameId;
            this.Players = new ConcurrentDictionary<GameClient, Player>();
            this._objects = new ConcurrentDictionary<uint, DynamicObject>();
            this._worlds = new ConcurrentDictionary<int, World>();

            this.StartingWorldSNOId = 71150; // FIXME: This must be set according to the game settings (start quest/act). Better yet, track the player's save point and toss this stuff. /komiga
            switch (clients[0].Account.CurrentGameAccount.CurrentToon.ActiveAct)
            {
                case 100:
                    StartingWorldSNOId = 161472;
                    break;
                case 200:
                    StartingWorldSNOId = 172909;
                    break;
                case 300:
                    StartingWorldSNOId = 178152;
                    break;
            }
            

            this.Quests = new QuestManager(this);

            this._tickWatch = new Stopwatch();
            var loopThread = new Thread(Update) { IsBackground = true, CurrentCulture = CultureInfo.InvariantCulture }; ; // create the game update thread.
            loopThread.Start();
            Pathfinder = new NullD.Core.GS.AI.Pather(this); //Creates the "Game"s single Pathfinder thread, Probably could be pushed further up and have a single thread handling all path req's for all running games. - DarkLotus
            var patherThread = new Thread(Pathfinder.UpdateLoop) { IsBackground = true, CurrentCulture = CultureInfo.InvariantCulture };
            patherThread.Start();
        }

        #region update & tick managment

        /// <summary>
        /// The main game loop.
        /// </summary>
        public void Update()
        {
            while (true)
            {
                this._tickWatch.Restart();
                Interlocked.Add(ref this._tickCounter, this.TickRate); // +6 ticks per 100ms. Verified by setting LogoutTickTimeMessage.Ticks to 600 which eventually renders a 10 sec logout timer on client. /raist

                // Lock Game instance to prevent incoming messages from modifying state while updating
                lock (this)
                {
                    // only update worlds with active players in it - so mob brain()'s in empty worlds doesn't get called and take actions for nothing. /raist.
                    foreach (var pair in this._worlds.Where(pair => pair.Value.HasPlayersIn))
                    {
                        pair.Value.Update(this._tickCounter);
                    }
                }

                this._tickWatch.Stop();

                var compensation = (int)(this.UpdateFrequency - this._tickWatch.ElapsedMilliseconds); // the compensation value we need to sleep in order to get consistent 100 ms Game.Update().

                if (this._tickWatch.ElapsedMilliseconds > this.UpdateFrequency)
                    Logger.Warn("Game.Update() took [{0}ms] more than Game.UpdateFrequency [{1}ms].", this._tickWatch.ElapsedMilliseconds, this.UpdateFrequency); // TODO: We may need to eventually use dynamic tickRate / updateFrenquencies. /raist.
                else
                    Thread.Sleep(compensation); // sleep until next Update().
            }
        }

        #endregion

        #region game-message handling & routing

        /// <summary>
        /// Routers incoming GameMessage to it's proper consumer.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public void Route(GameClient client, GameMessage message)
        {
            lock (this)
            {
                try
                {
                    switch (message.Consumer)
                    {
                        case Consumers.Game:
                            this.Consume(client, message);
                            break;
                        case Consumers.Inventory:
                            client.Player.Inventory.Consume(client, message);
                            break;
                        case Consumers.Player:
                            client.Player.Consume(client, message);
                            break;

                        case Consumers.Conversations:
                            client.Player.Conversations.Consume(client, message);
                            break;

                        case Consumers.SelectedNPC:
                            if (client.Player.SelectedNPC != null)
                                client.Player.SelectedNPC.Consume(client, message);
                            break;

                    }
                }
                catch (Exception e)
                {
                    Logger.DebugException(e, "Unhandled exception caught:");
                }
            }
        }

        public void Consume(GameClient client, GameMessage message)
        { } // for possile future messages consumed by game. /raist.

        #endregion

        #region player-handling

        /// <summary>
        /// Allows a player to join the game.
        /// </summary>
        /// <param name="joinedPlayer">The new player.</param>
        public void Enter(Player joinedPlayer)
        {
            this.Players.TryAdd(joinedPlayer.InGameClient, joinedPlayer);
            
            // send all players in the game to new player that just joined (including him)
            foreach (var pair in this.Players)
            {
                this.SendNewPlayerMessage(joinedPlayer, pair.Value);
            }

            // notify other players about our new player too.
            foreach (var pair in this.Players.Where(pair => pair.Value != joinedPlayer))
            {
                this.SendNewPlayerMessage(pair.Value, joinedPlayer);
            }

            var GameData = new GameSyncedData()
            {
                Field0 = 0x0,
                NowAct = 0x0,
                Field2 = 0x0,
                Field3 = 0x0,
                Field4 = 0x0,
                Field5 = 0x0,
                Field6 = 0x0,
                //Field7 = 0x0,
                Field7 = new[] { 0x0, 0x0 },
                Field8 = new[] { 0x0, 0x0 },
                //Field9 = new[] { 0x0, 0x0 },
                //Field10 = new[] { 0x0, 0x0 },
                //Field11 = new EntityId { High = 0,Low = 0 },
            };
            switch (joinedPlayer.Toon.ActiveAct)
            {
                case 100:
                    GameData.NowAct = 100;
                    StartingWorldSNOId = 161472;
                    break;
                case 200:
                    GameData.NowAct = 200;
                    StartingWorldSNOId = 172909;
                    break;
                case 300:
                    GameData.NowAct = 300;
                    StartingWorldSNOId = 178152;
                    break;
            }
            joinedPlayer.InGameClient.SendMessage(new GameSyncedDataMessage
            {
                Field0 = GameData
            });

            #region Загрузка лора игроков
            string LoreVar = joinedPlayer.Toon.LoreCollected;
            if (LoreVar != null & LoreVar != "")
            {
                string[] parts = LoreVar.Split(new char[] { '|' });
                foreach (var part in parts)
                    joinedPlayer.AddLoreFromBase(Convert.ToInt32(part));
            }
            #endregion
            if (joinedPlayer.PlayerIndex == 0)
            {
                if (joinedPlayer.Toon.ActiveAct == 0)
                {
                    #region Акт 1 Квест 1
                    if (joinedPlayer.Toon.ActiveQuest == 87700 & joinedPlayer.Toon.StepIDofQuest > 0)
                    {
                        StartingWorld.Leave(StartingWorld.GetActorByDynamicId(72));
                        //3739 - Rumford

                        var Capitan = StartingWorld.GetActorBySNO(3739);
                        Capitan.Attributes[Net.GS.Message.GameAttribute.MinimapActive] = true;
                        (Capitan as Core.GS.Actors.InteractiveNPC).Conversations.Clear();
                        (Capitan as Core.GS.Actors.InteractiveNPC).Conversations.Add(new Core.GS.Actors.Interactions.ConversationInteraction(198503));

                        Capitan.Attributes[Net.GS.Message.GameAttribute.Conversation_Icon, 0] = 1;
                        Capitan.Attributes.BroadcastChangedIfRevealed();

                    }
                    #endregion

                    #region Акт 1 Квест 2 - Наследие декарда каина
                    else if (joinedPlayer.Toon.ActiveQuest == 72095)
                    {

                        StartingWorld.Leave(StartingWorld.GetActorByDynamicId(72));

                        #region Перемотка ко второму квесту
                        for (int Rem = 0; Rem < 8; Rem++)
                        {
                            StartingWorld.Game.Quests.Advance(87700);
                        }
                        StartingWorld.Game.Quests.NotifyQuest(87700, NullD.Common.MPQ.FileFormats.QuestStepObjectiveType.InteractWithActor, 192164);
                        StartingWorld.Game.Quests.NotifyQuest(87700, NullD.Common.MPQ.FileFormats.QuestStepObjectiveType.HadConversation, 198521);

                        #endregion


                        //Берем нужную Лию =)
                        var LeahBrains = StartingWorld.GetActorByDynamicId(83);

                        Player MasterPlayer = joinedPlayer;
                        if (LeahBrains != null)
                        {
                            Logger.Debug("Вышибаем SNO {0}, мир содершит {1} ", LeahBrains.ActorSNO, StartingWorld.GetActorsBySNO(3739).Count);
                            StartingWorld.Leave(LeahBrains);


                        }
                        if (joinedPlayer.Toon.StepIDofQuest == -1 || joinedPlayer.Toon.StepIDofQuest == 28)
                        {
                            try
                            {
                                Hireling LeahFriend = new LeahParty(StartingWorld, LeahBrains.ActorSNO.Id, LeahBrains.Tags);
                                LeahFriend.Brain = new HirelingBrain(LeahFriend);

                                LeahFriend.GBHandle.Type = 4;
                                LeahFriend.GBHandle.GBID = 717705071;
                                LeahFriend.Attributes[GameAttribute.Pet_Creator] = joinedPlayer.PlayerIndex;
                                LeahFriend.Attributes[GameAttribute.Pet_Type] = 0x8;
                                LeahFriend.Attributes[GameAttribute.Hitpoints_Max] = 100f;
                                LeahFriend.Attributes[GameAttribute.Hitpoints_Cur] = 80f;
                                LeahFriend.Attributes[GameAttribute.Attacks_Per_Second] = 1.6f;
                                LeahFriend.Attributes[GameAttribute.Pet_Owner] = joinedPlayer.PlayerIndex;
                                LeahFriend.Attributes[GameAttribute.Untargetable] = false;
                                LeahFriend.Position = joinedPlayer.Position;
                                LeahFriend.RotationW = LeahBrains.RotationW;
                                LeahFriend.RotationAxis = LeahBrains.RotationAxis;
                                LeahFriend.EnterWorld(LeahBrains.Position);
                                LeahFriend.Attributes[GameAttribute.Level]++;
                                joinedPlayer.ActiveHireling = LeahFriend;
                                LeahFriend.Brain.Activate();
                                MasterPlayer = joinedPlayer;
                                StartingWorld.Leave(LeahBrains);


                                var NewTristramPortal = StartingWorld.GetActorBySNO(223757);
                                var ListenerUsePortalTask = Task<bool>.Factory.StartNew(() => OnUseTeleporterListener(NewTristramPortal.DynamicID, StartingWorld));
                                ListenerUsePortalTask.ContinueWith(delegate //Once killed:
                                {
                                    Logger.Debug(" Waypoint_NewTristram Objective done "); // Waypoint_NewTristram

                                });
                                var ListenerEnterToOldTristram = Task<bool>.Factory.StartNew(() => OnListenerToEnter(joinedPlayer, StartingWorld));

                                ListenerEnterToOldTristram.ContinueWith(delegate //Once killed:
                                {
                                    Logger.Debug("Enter to Road Objective done "); // Waypoint_OldTristram
                                    var ListenerEnterToAdriaEnter = Task<bool>.Factory.StartNew(() => OnListenerToAndriaEnter(joinedPlayer, StartingWorld));
                                });

                            }
                            catch
                            {
                                Logger.Warn("Ошибка создания спутника");

                                var NewTristramPortal = StartingWorld.GetActorBySNO(223757);
                                var ListenerUsePortalTask = Task<bool>.Factory.StartNew(() => OnUseTeleporterListener(NewTristramPortal.DynamicID, StartingWorld));
                                ListenerUsePortalTask.ContinueWith(delegate //Once killed:
                                {
                                    Logger.Debug(" Waypoint_NewTristram Objective done "); // Waypoint_NewTristram

                                });
                                var ListenerEnterToOldTristram = Task<bool>.Factory.StartNew(() => OnListenerToEnter(joinedPlayer, StartingWorld));

                                ListenerEnterToOldTristram.ContinueWith(delegate //Once killed:
                                {
                                    Logger.Debug("Enter to Road Objective done "); // Waypoint_OldTristram
                                    var ListenerEnterToAdriaEnter = Task<bool>.Factory.StartNew(() => OnListenerToAndriaEnter(joinedPlayer, StartingWorld));
                                });
                            }
                        }
                        else if (joinedPlayer.Toon.StepIDofQuest == 51)
                        {
                            joinedPlayer.Toon.StepOfQuest = 6;
                            var Gate = StartingWorld.GetActorBySNO(108466);
                            Gate.Destroy();
                            StartingWorld.Leave(LeahBrains);
                        }
                        else
                        {
                            StartingWorld.Leave(LeahBrains);
                            LeahBrains.EnterWorld(LeahBrains.Position);
                        }
                    }

                    #endregion

                    #region Акт 1 Квест 3 - Сломанная корона
                    else if (joinedPlayer.Toon.ActiveQuest == 72221)
                    {
                        StartingWorld.Leave(StartingWorld.GetActorByDynamicId(72));

                        #region Перемотка ко второму квесту
                        for (int Rem = 0; Rem < 8; Rem++)
                        {
                            StartingWorld.Game.Quests.Advance(87700);
                        }
                        StartingWorld.Game.Quests.NotifyQuest(87700, NullD.Common.MPQ.FileFormats.QuestStepObjectiveType.InteractWithActor, 192164);
                        StartingWorld.Game.Quests.NotifyQuest(87700, NullD.Common.MPQ.FileFormats.QuestStepObjectiveType.HadConversation, 198521);
                        #endregion
                        #region Перемотка ко третьему квесту
                        for (int Rem = 0; Rem < 15; Rem++)
                        {
                            StartingWorld.Game.Quests.Advance(72095);
                        }
                        StartingWorld.Game.Quests.NotifyQuest(72095, NullD.Common.MPQ.FileFormats.QuestStepObjectiveType.HadConversation, 198617);

                        var NewLeah = StartingWorld.GetActorByDynamicId(25);
                        foreach (var Conv in (NewLeah as InteractiveNPC).Conversations)
                        {
                            if (Conv.ConversationSNO == 198617)
                            {
                                Conv.Read = true;
                            }
                        }
                        #endregion
                        if (joinedPlayer.Toon.StepIDofQuest == -1)
                        {
                            var BlacksmithQuest = StartingWorld.GetActorBySNO(65036);
                            (BlacksmithQuest as Core.GS.Actors.InteractiveNPC).Conversations.Clear();
                            (BlacksmithQuest as Core.GS.Actors.InteractiveNPC).Conversations.Add(new Core.GS.Actors.Interactions.ConversationInteraction(198292));
                            (BlacksmithQuest as Core.GS.Actors.InteractiveNPC).Attributes[Net.GS.Message.GameAttribute.Conversation_Icon, 0] = 1;
                            (BlacksmithQuest as Core.GS.Actors.InteractiveNPC).Attributes.BroadcastChangedIfRevealed();
                        }
                        else
                        {
                            joinedPlayer.Toon.StepOfQuest = 5;
                        }
                        //Удаляем ненужныю корону.
                        foreach (var plr in StartingWorld.Game.Players.Values)
                        {
                            var inventory = plr.Inventory;
                            foreach (var itm in inventory.GetBackPackItems())
                            {
                                if (itm.ActorSNO.Id == 92168 || itm.ActorSNO.Id == 5356)
                                {
                                    inventory.DestroyInventoryItem(itm);
                                    inventory.RefreshInventoryToClient();
                                }
                            }
                        }

                    }
                    #endregion

                }
            }
            #region Прохождение игры 2.0 и Хард Фиксы
            if (joinedPlayer.Toon.ActiveQuest != -1)
            {
                if (joinedPlayer.Toon.ActiveAct == 0)
                {
                    joinedPlayer.EnterWorld(StartingWorld.StartingPoints.First().Position);

                    if (joinedPlayer.Toon.ActiveQuest != 87700)
                    {
                        #region Нижнии ворота тристрама
                        var DownGate = StartingWorld.GetActorBySNO(90419);
                        DownGate.Field2 = 16;
                        DownGate.Attributes[GameAttribute.Gizmo_State] = 1;
                        DownGate.Attributes[GameAttribute.Untargetable] = true;
                        DownGate.Attributes.BroadcastChangedIfRevealed();
                        StartingWorld.BroadcastIfRevealed(new Net.GS.Message.Definitions.Animation.SetIdleAnimationMessage
                        {
                            ActorID = DownGate.DynamicID,
                            AnimationSNO = Core.GS.Common.Types.TagMap.AnimationSetKeys.Open.ID
                        }, DownGate);
                        #endregion
                        //Убираем телегу
                        var FactorToShoot = StartingWorld.GetActorBySNO(81699);
                        FactorToShoot.Destroy();
                    }

                    #region Убираем телегу или делаем её нормальной.
                    if (joinedPlayer.Toon.ActiveQuest != 87700 && joinedPlayer.Toon.ActiveQuest != 72095 && joinedPlayer.Toon.ActiveQuest != -1)
                    {
                        var TELEGAS = StartingWorld.GetActorsBySNO(112131);
                        foreach (var TELEGA in TELEGAS)
                            TELEGA.Destroy();
                    }
                    else
                    {
                        var TELEGAS = StartingWorld.GetActorsBySNO(112131);
                        foreach (var TELEGA in TELEGAS)
                            TELEGA.Field2 = 0;
                    }
                    #endregion

                    if (joinedPlayer.Toon.StepIDofQuest != -1)
                        StartingWorld.Game.Quests[joinedPlayer.Toon.ActiveQuest].SwitchToStep(joinedPlayer.Toon.ActiveQuest, joinedPlayer.Toon.StepIDofQuest);
                    else
                        StartingWorld.Game.Quests[joinedPlayer.Toon.ActiveQuest].Advance();
                    if (joinedPlayer.Toon.ActiveQuest == 72546)
                        if (joinedPlayer.Toon.StepIDofQuest == 36)
                            joinedPlayer.Toon.StepOfQuest = 10;
                    if (joinedPlayer.Toon.ActiveQuest == 72061)
                        if (joinedPlayer.Toon.StepIDofQuest == 44)
                            StartingWorld.Game.Quests.NotifyQuest(72061, NullD.Common.MPQ.FileFormats.QuestStepObjectiveType.EnterWorld, 50585);

                }
                if (joinedPlayer.Toon.ActiveAct == 100)
                {
                    var a = StartingWorld.StartingPoints;
                    joinedPlayer.EnterWorld(this.StartingWorld.StartingPoints.Find(x => x.ActorSNO.Name == "Start_Location_Team_0").Position);
                }
                if (joinedPlayer.Toon.ActiveAct == 200)
                {
                    joinedPlayer.EnterWorld(StartingWorld.StartingPoints.Last().Position);
                }
                if (joinedPlayer.Toon.ActiveAct == 300)
                {
                    joinedPlayer.EnterWorld(StartingWorld.StartingPoints.Last().Position);
                }
            }
            else
            {
                joinedPlayer.EnterWorld(this.StartingWorld.StartingPoints.Find(x => x.ActorSNO.Name == "Start_Location_Team_0").Position);
                bool questConversation = true;
                var Capitan = this.StartingWorld.GetActorBySNO(3739);
                Capitan.Attributes[Net.GS.Message.GameAttribute.MinimapActive] = true;
                (Capitan as Core.GS.Actors.InteractiveNPC).Conversations.Clear();
                (Capitan as Core.GS.Actors.InteractiveNPC).Conversations.Add(new Core.GS.Actors.Interactions.ConversationInteraction(151087));

                Capitan.Attributes[Net.GS.Message.GameAttribute.Conversation_Icon, 0] = questConversation ? 1 : 0;
                Capitan.Attributes.BroadcastChangedIfRevealed();

                //Убираем эту заразу) wretchedMother
                StartingWorld.Leave(StartingWorld.GetActorBySNO(176889));
            }
            joinedPlayer.Toon.Side_StepOfQuest = 0;
            #endregion

            //Запуск игры
            joinedPlayer.InGameClient.TickingEnabled = true; // it seems bnet-servers only start ticking after player is completely in-game. /raist
        }

        #region Отслеживания для Акт 1 - Квест 2
        private bool OnUseTeleporterListener(uint actorDynID, Core.GS.Map.World world)
        {
            if (world.HasActor(actorDynID))
            {
                var actor = world.GetActorByDynamicId(actorDynID); // it is not null :p

                //Logger.Debug(" supposed portal has type {3} has name {0} and state {1} , has gizmo  been operated ? {2} ", actor.NameSNOId, actor.Attributes[Net.GS.Message.GameAttribute.Gizmo_State], actor.Attributes[Net.GS.Message.GameAttribute.Gizmo_Has_Been_Operated], actor.GetType());

                while (true)
                {
                    if (actor.Attributes[Net.GS.Message.GameAttribute.Gizmo_Has_Been_Operated] == true)
                    {
                        world.Game.Quests.Advance(72095);
                        foreach (var playerN in world.Players.Values)
                        {
                            playerN.Toon.ActiveQuest = 72095;
                            //dbQuestProgress.StepIDofQuest = 28;
                        }
                        break;
                    }
                }
            }
            return true;
        }
        private bool OnListenerToEnterScene(Core.GS.Players.Player player, Core.GS.Map.World world, int SceneID)
        {
            while (true)
            {
                try
                {
                    int NOWsceneID = player.CurrentScene.SceneSNO.Id;
                    if (NOWsceneID == SceneID)
                    {

                        break;
                    }
                }
                catch { Logger.Debug("Приостановка скрипта, идёт загрузка."); }
            }
            return true;
        }


        private bool OnListenerToEnter(Core.GS.Players.Player player, Core.GS.Map.World world)
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
                                var facingAngle = Core.GS.Actors.Movement.MovementHelpers.GetFacingAngle(player.ActiveHireling.Position, NearDoor);
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
                            world.Game.Quests.NotifyQuest(72095, NullD.Common.MPQ.FileFormats.QuestStepObjectiveType.EnterTrigger, -1);
                            break;
                        }
                    }
                }
                catch { }
            }

            return true;
        }
        private bool OnKillListenerCain(List<uint> monstersAlive, Core.GS.Map.World world)
        {
            Int32 monstersKilled = 0;
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
        #endregion

        private bool StartConversation(Core.GS.Map.World world, Int32 conversationId)
        {
            foreach (var player in world.Players)
            {
                player.Value.Conversations.StartConversation(conversationId);
            }
            return true;
        }
        /// <summary>
        /// Sends NewPlayerMessage to players when a new player joins the game. 
        /// </summary>
        /// <param name="target">Target player to send the message.</param>
        /// <param name="joinedPlayer">The new joined player.</param>
        private void SendNewPlayerMessage(Player target, Player joinedPlayer)
        {
            target.InGameClient.SendMessage(new NewPlayerMessage
            {
                PlayerIndex = joinedPlayer.PlayerIndex, // player index
                ToonId = new EntityId() { High = (long)joinedPlayer.Toon.D3EntityID.IdHigh, Low = (long)joinedPlayer.Toon.D3EntityID.IdLow }, //Toon
                GameAccountId = new EntityId() { High = (long)joinedPlayer.Toon.GameAccount.BnetEntityId.High, Low = (long)joinedPlayer.Toon.GameAccount.BnetEntityId.Low }, //GameAccount
                ToonName = joinedPlayer.Toon.Name,
                Field3 = 0x00000002, //party frame class
                Field4 = target != joinedPlayer ? 0x2 : 0x4, //party frame level /boyc - may mean something different /raist.
                snoActorPortrait = joinedPlayer.ClassSNO, //party frame portrait
                Level = joinedPlayer.Toon.Level,
                AltLevel = 0,
                StateData = joinedPlayer.GetStateData(),
                Field8 = this.Players.Count != 1, //announce party join
                Field9 = 0x00000001,
                ActorID = joinedPlayer.DynamicID,
            });

            var dbArtisans = DBSessions.AccountSession.Get<DBArtisansOfToon>(joinedPlayer.Toon.PersistentID);
            target.InGameClient.SendMessage(joinedPlayer.GetPlayerBanner()); // send player banner proto - D3.GameMessage.PlayerBanner
            if (target.Toon.MaximumQuest != -1 &
              target.Toon.MaximumQuest != 87000 &
              target.Toon.MaximumQuest != 72095 &
              target.Toon.MaximumQuest != 72221)
            {
                target.InGameClient.SendMessage(joinedPlayer.GetBlacksmithData(dbArtisans)); // Modded by AiDiEvE
            }
            else
            {
                target.InGameClient.SendMessage(joinedPlayer.GetBlacksmithDataFixInt(0));
                StartingWorld.Leave(StartingWorld.GetActorBySNO(56947)); //Кузнец
                StartingWorld.Leave(StartingWorld.GetActorBySNO(4062));  //Чародейка
                StartingWorld.Leave(StartingWorld.GetActorBySNO(4644));  //Хитрожопый)
                StartingWorld.Leave(StartingWorld.GetActorBySNO(4538));  //Храмовник
            }
            target.InGameClient.SendMessage(joinedPlayer.GetJewelerData(dbArtisans));
            target.InGameClient.SendMessage(joinedPlayer.GetMysticData(dbArtisans));

            DBSessions.AccountSession.Flush();
        }

        #endregion

        #region object dynamicId tracking

        public void StartTracking(DynamicObject obj)
        {
            if (obj.DynamicID == 0 || IsTracking(obj))
                throw new Exception(String.Format("Object has an invalid ID or was already being tracked (ID = {0})", obj.DynamicID));
            this._objects.TryAdd(obj.DynamicID, obj);
        }

        public void EndTracking(DynamicObject obj)
        {
            if (obj.DynamicID == 0 || !IsTracking(obj))
                throw new Exception(String.Format("Object has an invalid ID or was not being tracked (ID = {0})", obj.DynamicID));

            DynamicObject removed;
            this._objects.TryRemove(obj.DynamicID, out removed);
        }

        public DynamicObject GetObject(uint dynamicID)
        {
            DynamicObject obj;
            this._objects.TryGetValue(dynamicID, out obj);
            return obj;
        }

        public bool IsTracking(uint dynamicID)
        {
            return this._objects.ContainsKey(dynamicID);
        }

        public bool IsTracking(DynamicObject obj)
        {
            return this._objects.ContainsKey(obj.DynamicID);
        }

        #endregion

        #region world collection

        public void AddWorld(World world)
        {
            if (world.WorldSNO.Id == -1 || WorldExists(world.WorldSNO.Id))
                throw new Exception(String.Format("World has an invalid SNO or was already being tracked (ID = {0}, SNO = {1})", world.DynamicID, world.WorldSNO.Id));
            this._worlds.TryAdd(world.WorldSNO.Id, world);
        }

        public void RemoveWorld(World world)
        {
            if (world.WorldSNO.Id == -1 || !WorldExists(world.WorldSNO.Id))
                throw new Exception(String.Format("World has an invalid SNO or was not being tracked (ID = {0}, SNO = {1})", world.DynamicID, world.WorldSNO.Id));

            World removed;
            this._worlds.TryRemove(world.WorldSNO.Id, out removed);
        }

        public World GetWorld(int worldSNO)
        {
            World world;
            this._worlds.TryGetValue(worldSNO, out world);

            if (world == null) // If it doesn't exist, try to load it
            {
                world = WorldGenerator.Generate(this, worldSNO);
                if (world == null) Logger.Warn("Failed to generate world with sno: {0}", worldSNO);
            }
            return world;
        }

        public bool WorldExists(int worldSNO)
        {
            return this._worlds.ContainsKey(worldSNO);
        }

        #endregion

    }
}
