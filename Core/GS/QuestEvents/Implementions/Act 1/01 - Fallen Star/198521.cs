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
using NullD.Core.GS.Map;
using NullD.Core.GS.Common.Types.TagMap;
using NullD.Core.GS.Actors.Interactions;
using NullD.Core.GS.Actors;

namespace NullD.Core.GS.QuestEvents.Implementations
{
    class _198521 : QuestEvent  // RumfordProtectorEnd_New and be careful as this shit is also supposed to trigger the next event with leah..TEH HELL
    {

        private static readonly Logger Logger = LogManager.CreateLogger();
        public List<ConversationInteraction> Conversations { get; private set; }
        private Boolean HadConversation = true;


        public _198521()
            : base(198521)
        {
        }

        public override void Execute(Map.World world)
        {
            Logger.Debug(" Conversation done ");
            if (HadConversation)
            {
                world.Game.Quests.NotifyQuest(87700, NullD.Common.MPQ.FileFormats.QuestStepObjectiveType.HadConversation, 198521);
                HadConversation = false;
            }

            foreach (var plr in world.Players.Values)
            {
                plr.Toon.ActiveQuest = 72095;
                plr.Toon.StepOfQuest = -1;
                plr.Toon.StepIDofQuest = -1;

                plr.UpdateHeroState();
            };

            Logger.Debug("ПОЛУЧЕНИЕ НАГРАДЫ!");
            foreach (var player in world.Game.Players.Values)
            {
                D3.Quests.QuestReward.Builder Reward = new D3.Quests.QuestReward.Builder();
                Reward.SnoQuest = 87700;
                Reward.GoldGranted = 370;
                Reward.XpGranted = 1125;

                D3.Quests.QuestStepComplete.Builder StepCompleted = new D3.Quests.QuestStepComplete.Builder();
                StepCompleted.Reward = Reward.Build();
                StepCompleted.SetIsQuestComplete(true);

                player.InGameClient.SendMessage(new Net.GS.Message.Definitions.Quest.QuestStepCompleteMessage()
                {
                    QuestStepComplete = StepCompleted.Build()
                });
                player.Inventory.AddGoldAmount(Reward.GoldGranted);
                player.UpdateExp(Reward.XpGranted);

            }
            var LeahToConv = world.GetActorByDynamicId(83);
            LeahToConv.dumpConversationList();
            ConversationInteraction QuestCOnv = new ConversationInteraction(198541);
            (LeahToConv as InteractiveNPC).AddMustConversation();

        }

        //Launch Conversations.
        private bool StartConversation(Map.World world, Int32 conversationId)
        {
            foreach (var player in world.Players)
            {
                player.Value.Conversations.StartConversation(conversationId); // this does the job of sending the proper stuff :p
            }
            return true;
        }
    }
}