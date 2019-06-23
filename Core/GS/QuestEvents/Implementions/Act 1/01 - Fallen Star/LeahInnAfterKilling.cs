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
using System.Threading;
using System.Threading.Tasks;
using NullD.Common.MPQ.FileFormats;
using NullD.Net.GS.Message.Definitions.ACD;
using NullD.Net.GS.Message.Definitions.Animation;
using NullD.Core.GS.Actors;
using NullD.Core.GS.Common.Types.Math;
using NullD.Core.GS.Generators;
using NullD.Common.Logging;


namespace NullD.Core.GS.QuestEvents.Implementations
{
    class LeahInnAfterKilling : QuestEvent
    {
        //ActorID: 0x7A3100DD  
        //ZombieSkinny_A_LeahInn.acr (2050031837)
        //ActorSNOId: 0x00031971:ZombieSkinny_A_LeahInn.acr

        private static readonly Logger Logger = LogManager.CreateLogger();

        public LeahInnAfterKilling()
            : base(151167)
        {
        }

        private Boolean HadConversation = true;

        public override void Execute(Map.World world)
        {
            if (HadConversation)
            {
                HadConversation = false;

                world.Game.Quests.NotifyQuest(87700, QuestStepObjectiveType.HadConversation, 151167);
                foreach (var plr in world.Players.Values)
                {
                    plr.Toon.ActiveQuest = 87700;
                    plr.Toon.StepOfQuest = 5;
                    plr.Toon.StepIDofQuest = 50;
                };
            }

            bool questConversation = true;
            var TristramWorld = world.Game.GetWorld(71150);
            var Capitan = TristramWorld.GetActorBySNO(3739);
            Capitan.Attributes[Net.GS.Message.GameAttribute.MinimapActive] = true;
            (Capitan as Core.GS.Actors.InteractiveNPC).Conversations.Clear();
            (Capitan as Core.GS.Actors.InteractiveNPC).Conversations.Add(new Core.GS.Actors.Interactions.ConversationInteraction(198503));

            Capitan.Attributes[Net.GS.Message.GameAttribute.Conversation_Icon, 0] = questConversation ? 1 : 0;
            Capitan.Attributes.BroadcastChangedIfRevealed();
        }


    }
}