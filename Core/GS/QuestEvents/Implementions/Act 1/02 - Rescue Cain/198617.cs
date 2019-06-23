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
using NullD.Core.GS.Actors;

namespace NullD.Core.GS.QuestEvents.Implementations
{
    class _198617 : QuestEvent
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        public _198617()
            : base(198617)
        {
        }

        private Boolean HadConversation = true;

        public override void Execute(Map.World world)
        {
            if (HadConversation)
            {
                HadConversation = false;
            }
            foreach (var player in world.Players.Values)
            {
                player.Toon.MaximumQuest = 72221;
                player.Toon.ActiveQuest = 72221;
                player.Toon.StepOfQuest = -1;
            };
            Logger.Debug(" Второй квест окончен. ");


            bool questConversation = true;
            var Cain = world.GetActorBySNO(3533);
            Cain.Attributes[Net.GS.Message.GameAttribute.MinimapActive] = true;
            (Cain as InteractiveNPC).Conversations.Clear();
            (Cain as InteractiveNPC).Conversations.Add(new Actors.Interactions.ConversationInteraction(198691));

            Cain.Attributes[Net.GS.Message.GameAttribute.Conversation_Icon, 0] = questConversation ? 1 : 0;
            Cain.Attributes.BroadcastChangedIfRevealed();

            try
            {
                var BlacksmithVendor = world.GetActorBySNO(56947);
                Vector3D position = new Vector3D(BlacksmithVendor.Position);
                var BlacksmithQuest = world.GetActorBySNO(65036);
                BlacksmithQuest.RotationAxis = BlacksmithVendor.RotationAxis;
                BlacksmithQuest.RotationW = BlacksmithVendor.RotationW;

                var TELEGAS = world.GetActorsBySNO(112131);
                Vector3D LastTelega = new Vector3D();
                foreach (var TELEGA in TELEGAS)
                {
                    LastTelega = TELEGA.Position;
                    TELEGA.Destroy();
                }
            }
            catch { Logger.Warn("Не критичная ошибка скрипта."); }

            Players.Player Master = null;

            Logger.Debug("ПОЛУЧЕНИЕ НАГРАДЫ!");
            foreach (var player in world.Game.Players.Values)
            {
                if (player.PlayerIndex == 0)
                    Master = player;

                D3.Quests.QuestReward.Builder Reward = new D3.Quests.QuestReward.Builder();
                Reward.SnoQuest = 72095;
                Reward.XpGranted = 3300;

                D3.Quests.QuestStepComplete.Builder StepCompleted = new D3.Quests.QuestStepComplete.Builder();
                StepCompleted.Reward = Reward.Build();
                StepCompleted.SetIsQuestComplete(true);

                player.InGameClient.SendMessage(new Net.GS.Message.Definitions.Quest.QuestStepCompleteMessage()
                {
                    QuestStepComplete = StepCompleted.Build()
                });
                player.UpdateExp(Reward.XpGranted);
            }


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