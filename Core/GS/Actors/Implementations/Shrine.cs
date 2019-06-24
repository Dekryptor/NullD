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

using NullD.Core.GS.Map;
using NullD.Core.GS.Common.Types.TagMap;
using NullD.Net.GS.Message;
using NullD.Net.GS.Message.Definitions.Misc;

namespace NullD.Core.GS.Actors.Implementations
{
    /// <summary>
    /// Class that implements shrines, run power on click and send activation message
    /// </summary>
    class Shrine : Gizmo
    {
        public Shrine(World world, int snoId, TagMap tags)
            : base(world, snoId, tags)
        {
            Attributes[GameAttribute.MinimapActive] = true;
        }


        private bool WaitToRefresh(Ticker.TickTimer timer)
        {
            while (timer.TimedOut != true)
            {

            }
            return true;
        }

        public override void OnTargeted(Players.Player player, Net.GS.Message.Definitions.World.TargetMessage message)
        {

            World.BroadcastIfRevealed(new ShrineActivatedMessage { ActorID = this.DynamicID }, this);
            #region Анимация использования
            World.BroadcastIfRevealed(new Net.GS.Message.Definitions.Animation.PlayAnimationMessage
            {
                ActorID = this.DynamicID,
                Field1 = 11,
                Field2 = 0,
                tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
               {
                    new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                    {
                        Duration = 10,
                        AnimationSNO = AnimationSet.TagMapAnimDefault[AnimationSetKeys.DeathDefault],
                        PermutationIndex = 0,
                        Speed = 1
                    }
               }

            }, this);
            #endregion
            #region Исцеление
            var playersAffected = player.GetPlayersInRange(26f);
            foreach (Players.Player playerN in playersAffected)
            {
                foreach (Players.Player targetAffected in playersAffected)
                {
                    player.InGameClient.SendMessage(new Net.GS.Message.Definitions.Effect.PlayEffectMessage()
                    {
                        ActorId = targetAffected.DynamicID,
                        Effect = Net.GS.Message.Definitions.Effect.Effect.HealthOrbPickup
                    });
                }

                player.AddPercentageHP(100);
            }
            #endregion

            switch (this.ActorSNO.Id)
            {
                case 225269:
                    World.PowerManager.RunPower(player, 30476);//Shrine_Desecrated_Blessed - Благославение, противники наносят на 25% меньше урона
                    Logger.Warn("Shrine Heal, and activate Blessed Buff. (No Work)");
                    break;
                case 225270:
                    World.PowerManager.RunPower(player, 30477);//Shrine_Desecrated_Enlightened - Просветление, +25% к урону.
                    Logger.Warn("Shrine Heal, and activate Enlightened Buff. (Complete Work)");
                    break;
                case 225271:
                    World.PowerManager.RunPower(player, 30478);//Shrine_Desecrated_Fortune - Вероятность лучшего дропа +25%
                    Logger.Warn("Shrine Heal, and activate Fortune Buff. (Complete Work)");
                    break;
                case 225272:
                    World.PowerManager.RunPower(player, 30479);// Shrine_Desecrated_Frenzied
                    Logger.Warn("Shrine Heal, and activate Frenzied Buff. (Complete Work");
                    break;
            }

            this.Attributes[GameAttribute.Gizmo_Has_Been_Operated] = true;
            this.Attributes[GameAttribute.Gizmo_State] = 1;
            Attributes.BroadcastChangedIfRevealed();
            player.PlayEffectGroup(18364);

            //player.Attributes[GameAttribute.Skill, 226820] = 1;
            //player.Attributes[GameAttribute.Skill_Total, 226820] = 1;
            player.Attributes.SendChangedMessage(player.InGameClient);
        }
    }
}
