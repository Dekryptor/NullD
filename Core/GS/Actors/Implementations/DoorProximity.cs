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
using NullD.Core.GS.Map;
using NullD.Core.GS.Common.Types.TagMap;
using NullD.Net.GS.Message.Definitions.Animation;
using NullD.Core.GS.Common.Types.SNO;
using NullD.Net.GS.Message;
using TreasureClass = NullD.Common.MPQ.FileFormats.TreasureClass;
using NullD.Common.Storage;
using NullD.Common.Storage.AccountDataBase.Entities;
using NullD.Core.GS.Ticker;

namespace NullD.Core.GS.Actors.Implementations
{
    /// <summary>
    /// Class that implements behaviour for clickable door types.
    /// Play open animation on click, then set idle animation
    /// </summary>
    [HandledSNO(169502, 230324, 216574, 117344)]//81796



    class DoorProximity : Gizmo
    {
        private bool _collapsed = false;

        public DoorProximity(World world, int snoId, TagMap tags)
            : base(world, snoId, tags)
        {
            Field2 = 0;
        }

        public override void OnPlayerApproaching(Players.Player player)
        {
            if (player.Position.DistanceSquared(ref _position) < ActorData.Sphere.Radius * ActorData.Sphere.Radius * 2 * this.Scale * this.Scale && !_collapsed)
            {
                _collapsed = true;

                var OpenDoor = new Door(this.World, this.ActorSNO.Id, this.Tags);
                OpenDoor.Field2 = 16;
                OpenDoor.RotationAxis = this.RotationAxis;
                OpenDoor.RotationW = this.RotationW;
                OpenDoor.Attributes[GameAttribute.Gizmo_Has_Been_Operated] = true;
                OpenDoor.Attributes[GameAttribute.Gizmo_Operator_ACDID] = unchecked((int)player.DynamicID);
                OpenDoor.Attributes[GameAttribute.Gizmo_State] = 1;
                OpenDoor.Attributes[GameAttribute.Untargetable] = true;
                Attributes.BroadcastChangedIfRevealed();
                OpenDoor.EnterWorld(this.Position);

                World.BroadcastIfRevealed(new PlayAnimationMessage
                {
                    ActorID = OpenDoor.DynamicID,
                    Field1 = 5,
                    Field2 = 0,
                    tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                    {
                    new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                    {
                        Duration = 1500,
                        AnimationSNO = AnimationSet.TagMapAnimDefault[AnimationSetKeys.Opening],
                        PermutationIndex = 0,
                        Speed = 1
                    }
                    }

                }, OpenDoor);

                World.BroadcastIfRevealed(new SetIdleAnimationMessage
                {
                    ActorID = OpenDoor.DynamicID,
                    AnimationSNO = AnimationSetKeys.Open.ID
                }, OpenDoor);

                Destroy();


                //this.Attributes[GameAttribute.Deleted_On_Server] = true;

                //RelativeTickTimer destroy = new RelativeTickTimer(World.Game, duration, x => this.Destroy());
            }
        }
    }
}