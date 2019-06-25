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

using NullD.Common.Logging;
using NullD.Core.GS.Map;
using NullD.Core.GS.Common.Types.TagMap;
using NullD.Core.GS.AI.Brains;
using NullD.Core.GS.Common.Types.SNO;
using NullD.Core.GS.Objects;
using NullD.Net.GS.Message;
using MonsterFF = NullD.Common.MPQ.FileFormats.Monster;
using GameBalance = NullD.Common.MPQ.FileFormats.GameBalance;
using NullD.Core.GS.Players;
using NullD.Net.GS.Message.Definitions.World;
using NullD.Net.GS.Message.Definitions.Effect;

namespace NullD.Core.GS.Actors.Implementations
{
    [HandledSNO(141246, //Prist in Tristram
                226345, //Prist in Bastion
                226343)] //Prist_Caldeum
    class Healer : InteractiveNPC
    {
        public Healer(World world, int snoId, TagMap tags)
            : base(world, snoId, tags)
        {
            if (snoId == 226343)
            {
                (this as InteractiveNPC).Conversations.Clear();
            }
            this.Field7 = 1;
            this.Attributes[GameAttribute.MinimapActive] = true;
        }

        public override void OnTargeted(Player player, TargetMessage message)
        {
            var playersAffected = player.GetPlayersInRange(26f);
            foreach (Player plr in playersAffected)
            {
                foreach (Player targetAffected in playersAffected)
                {
                    plr.InGameClient.SendMessage(new PlayEffectMessage()
                    {
                        ActorId = targetAffected.DynamicID,
                        Effect = Effect.HealthOrbPickup
                    });
                }
                //every summon and mercenary owned by you must broadcast their green text to you /H_DANILO
                plr.AddPercentageHP(100);
            }
            base.OnTargeted(player, message);
        }
    }
}