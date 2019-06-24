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
using NullD.Core.GS.Players;
using NullD.Core.GS.Map;
using NullD.Net.GS.Message.Definitions.World;
using NullD.Core.GS.Common.Types.TagMap;

namespace NullD.Core.GS.Items.Implementations
{
    [HandledType("Book")]
    public class Book : Item
    {
        public static readonly Logger Logger = LogManager.CreateLogger();

        public int LoreSNOId { get; private set; }

        public Book(GS.Map.World world, NullD.Common.MPQ.FileFormats.ItemTable definition, bool Craft = false)
            : base(world, definition)
        {
            var actorData = ActorSNO.Target as NullD.Common.MPQ.FileFormats.Actor;

            if (actorData.TagMap.ContainsKey(ActorKeys.Lore))
            {
                LoreSNOId = actorData.TagMap[ActorKeys.Lore].Id;
            }
        }

        public override void OnTargeted(Player player, TargetMessage message)
        {
            //Logger.Trace("OnTargeted");
            if (LoreSNOId != -1)
            {
                player.PlayLore(LoreSNOId, true);
            }
            if (player.GroundItems.ContainsKey(this.DynamicID))
                player.GroundItems.Remove(this.DynamicID);
            this.Destroy();
        }
    }
}