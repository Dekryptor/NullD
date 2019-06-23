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
using NullD.Common.Storage;
using NullD.Common.Storage.AccountDataBase.Entities;

namespace NullD.Core.GS.QuestEvents.Implementations
{
    class _198588 : QuestEvent
    {
        private static readonly Logger Logger = LogManager.CreateLogger();
        private Boolean HadConversation = true;

        public _198588()
            : base(198588)
        {
        }

        public override void Execute(Map.World world)
        {
            Logger.Debug(" Разговор с Леей закончен ");
            if (HadConversation)
                world.Game.Quests.NotifyQuest(72095, QuestStepObjectiveType.PossessItem, -1);
            //world.Game.Quests.Advance(72095);
            foreach (var player in world.Players)
            {
                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Value.Toon.PersistentID);
                dbQuestProgress.ActiveQuest = 72095;
                dbQuestProgress.StepOfQuest = 9;
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();
            };

            var Leah_Cellar = world.GetActorBySNO(203030);
            (Leah_Cellar as Actors.InteractiveNPC).Conversations.Clear();
            Leah_Cellar.Attributes[Net.GS.Message.GameAttribute.Conversation_Icon, 0] = 0;
            Leah_Cellar.Position = new Vector3D(149.8516f, 60.33301f, 9000f);
            Leah_Cellar.Attributes.BroadcastChangedIfRevealed();
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