/*
 * Copyright (C) 2011 NullD project
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

using System.Collections.Generic;
using System.Linq;
using NullD.Common.Helpers;
using NullD.Common.Helpers.Math;
using NullD.Common.MPQ.FileFormats.Types;
using NullD.Core.GS.Map;
using NullD.Core.GS.Objects;
using NullD.Core.GS.Players;
using NullD.Net.GS.Message;
using NullD.Net.GS.Message.Definitions.World;
using NullD.Net.GS.Message.Fields;
using NullD.Net.GS.Message.Definitions.Animation;
using NullD.Net.GS.Message.Definitions.Effect;
using NullD.Net.GS.Message.Definitions.Misc;
using NullD.Common.MPQ;
using NullD.Core.GS.Common.Types.SNO;
using System;
using NullD.Core.GS.Common.Types.TagMap;
using MonsterFF = NullD.Common.MPQ.FileFormats.Monster;
using ActorFF = NullD.Common.MPQ.FileFormats.Actor;
using NullD.Core.GS.AI.Brains;
using NullD.Core.GS.Ticker;


namespace NullD.Core.GS.Actors
{
    public class Minion : Living, IUpdateable
    {
        public Actor Master; //The player who summoned the minion.

        public override ActorType ActorType { get { return ActorType.Monster; } }

        public override int Quality
        {
            get
            {
                return (int)NullD.Common.MPQ.FileFormats.SpawnType.Normal; //Seems like this was never implemented on the clientside, so using 0 is fine.
            }
            set
            {
                // Not implemented
            }
        }

        public Minion(World world, int snoId, Actor master, TagMap tags)
            : base(world, snoId, tags)
        {
            // The following two seems to be shared with monsters. One wonders why there isn't a specific actortype for minions.
            this.Master = master;
            this.Field2 = 0x8;
            this.GBHandle.Type = (int)GBHandleType.Monster; this.GBHandle.GBID = 1;
            this.Attributes[GameAttribute.Summoned_By_ACDID] = (int)master.DynamicID;
            this.Attributes[GameAttribute.TeamID] = master.Attributes[GameAttribute.TeamID];
        }

        public override void OnTargeted(Player player, TargetMessage message)
        {
        }

        public void Update(int tickCounter)
        {
            if (this.Brain == null)
                return;

            this.Brain.Update(tickCounter);
        }

        public void SetBrain(NullD.Core.GS.AI.Brain brain)
        {
            this.Brain = brain;
        }

        public void AddPresetPower(int powerSNO)
        {
            (Brain as MinionBrain).AddPresetPower(powerSNO);
        }
    }
}
