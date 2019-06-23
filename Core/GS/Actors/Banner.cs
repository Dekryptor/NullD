using NullD.Core.GS.Common.Types.TagMap;
using NullD.Core.GS.Map;
using NullD.Net.GS.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullD.Core.GS.Actors
{
    class Banner : Gizmo
    {
        public Banner(World world, int snoId, TagMap tags)
            : base(world, snoId, tags)
        {
            Attributes[GameAttribute.Gizmo_State] = 0;
            Field2 = 0;
        }

        public override void OnTargeted(Players.Player player, Net.GS.Message.Definitions.World.TargetMessage message)
        {
            Logger.Warn("Baner System ver 0.2");

            #region Активация баннера игрока для телепортации
            if (this.ActorSNO.Name == "Banner_Player_1")
            {
                foreach (var targetplayer in this.World.Game.Players.Values)
                {
                    if (targetplayer.PlayerIndex == 0)
                        if (targetplayer.Position != player.Position)
                        {
                            player.Teleport(targetplayer.Position);
                            Logger.Warn("Перенос пользователя с помощью флага к игроку № {0}", player.PlayerIndex);
                        }
                }
            }
            else if (this.ActorSNO.Name == "Banner_Player_2")
            {
                foreach (var targetplayer in this.World.Game.Players.Values)
                {
                    if (targetplayer.PlayerIndex == 1)
                        if (targetplayer.Position != player.Position)
                        {
                            player.Teleport(targetplayer.Position);
                            Logger.Warn("Перенос пользователя с помощью флага к игроку № {0}", player.PlayerIndex);
                        }
                }
            }
            else if (this.ActorSNO.Name == "Banner_Player_3")
            {
                foreach (var targetplayer in this.World.Game.Players.Values)
                {
                    if (targetplayer.PlayerIndex == 2)
                        if (targetplayer.Position != player.Position)
                        {
                            player.Teleport(targetplayer.Position);
                            Logger.Warn("Перенос пользователя с помощью флага к игроку № {0}", player.PlayerIndex);
                        }
                }
            }
            else if (this.ActorSNO.Name == "Banner_Player_4")
            {
                foreach (var targetplayer in this.World.Game.Players.Values)
                {
                    if (targetplayer.PlayerIndex == 3)
                        if (targetplayer.Position != player.Position)
                        {
                            player.Teleport(targetplayer.Position);
                            Logger.Warn("Перенос пользователя с помощью флага к игроку № {0}", player.PlayerIndex);
                        }
                }
            }
            #endregion

        }
    }
}
