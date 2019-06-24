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

using NullD.Core.GS.Map;
using NullD.Core.GS.Common.Types.TagMap;
using NullD.Net.GS.Message;
using NullD.Net.GS.Message.Definitions.Animation;
using NullD.Common.Storage;
using System;

namespace NullD.Core.GS.Actors.Implementations
{
    /// <summary>
    /// Class that implements healthwell, run power on click and change gizmo state
    /// </summary>
    [HandledSNO(131123)] //Котёл
    class Boiler : Gizmo
    {
        public Boiler(World world, int snoId, TagMap tags)
            : base(world, snoId, tags)
        {
            Attributes[GameAttribute.Gizmo_State] = 0;
            Field2 = 0;
            this.Field7 = 1;
        }

        private bool WaitToSpawn(Ticker.TickTimer timer)
        {
            while (timer.TimedOut != true)
            {

            }
            return true;
        }

        public override void OnTargeted(Players.Player player, Net.GS.Message.Definitions.World.TargetMessage message)
        {
            foreach (var plr in World.Game.Players.Values)
            {
                if (plr.PlayerIndex == 0 & plr.Toon.ActiveQuest == 72095)
                {
                    plr.Toon.ActiveQuest = 72095;
                    if (plr.Toon.StepOfQuest == 5)
                    {
                        plr.Toon.StepIDofQuest = 43;
                        StartConversation(World, 167115);
                        World.Game.Quests.Advance(72095);
                    }
                }
            }

            #region Отклик и анимация.
            World.BroadcastIfRevealed(new PlayAnimationMessage
            {
                ActorID = this.DynamicID,
                Field1 = 5,
                Field2 = 0,
                tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                   {
                    new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                    {
                        Duration = 50,
                        AnimationSNO = AnimationSet.TagMapAnimDefault[AnimationSetKeys.Opening],
                        PermutationIndex = 0,
                        Speed = 1
                    }
                   }

            }, this);

            World.BroadcastIfRevealed(new SetIdleAnimationMessage
            {
                ActorID = this.DynamicID,
                AnimationSNO = AnimationSetKeys.Open.ID
            }, this);

            this.Attributes[GameAttribute.Gizmo_Has_Been_Operated] = true;
            this.Attributes[GameAttribute.Gizmo_Operator_ACDID] = unchecked((int)player.DynamicID);
            this.Attributes[GameAttribute.Chest_Open, 0xFFFFFF] = true;
            Attributes.BroadcastChangedIfRevealed();

            base.OnTargeted(player, message);
            #endregion
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
