﻿/*
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

using System.Windows;
using NullD.Common.MPQ;
using NullD.Core.GS.Common.Types.SNO;
using NullD.Core.GS.Map;
using NullD.Core.GS.Players;
using NullD.Net.GS.Message;
using NullD.Net.GS.Message.Definitions.Animation;
using NullD.Net.GS.Message.Definitions.Map;
using NullD.Net.GS.Message.Definitions.Misc;
using NullD.Net.GS.Message.Fields;
using NullD.Core.GS.Common.Types.TagMap;
using NullD.Core.GS.Common.Types.Math;

namespace NullD.Core.GS.Actors.Implementations
{
    public sealed class Waypoint : Gizmo
    {
        public int WaypointId { get; private set; }
        public int LastI;

        public Waypoint(World world, int snoId, TagMap tags)
            : base(world, snoId, tags)
        {

            /*
                [072689] [Actor] MinimapIconStairs_Switch
                [075172] [Actor] minimapMarker_model
                [004686] [Actor] MinimapIconStairs
                [075171] [Appearance] minimapMarker_model
                [212733] [MarkerSet] caOut_Boneyard_ExitA_E02_S02 (Minimap Pings)

             */

            var FalseActorToIcons = World.GetActorsBySNO(4686);
            foreach (var one in FalseActorToIcons)
            {
                one.Attributes[GameAttribute.MinimapIconOverride] = 0x1FA21;
                one.Attributes[GameAttribute.MinimapActive] = true;
                one.Attributes.BroadcastChangedIfRevealed();
            }
            this.Attributes[GameAttribute.MinimapIconOverride] = 0x1FA21;
            this.Attributes[GameAttribute.MinimapActive] = true;

        }

        public override void OnEnter(World world)
        {
            this.ReadWaypointId(world);
        }

        private void ReadWaypointId(World world)
        {
            var actData = (NullD.Common.MPQ.FileFormats.Act)MPQStorage.Data.Assets[SNOGroup.Act][70015].Data;
            #region Проверка при старте

            if (world.WorldSNO.Id == 71150)
                actData = (NullD.Common.MPQ.FileFormats.Act)MPQStorage.Data.Assets[SNOGroup.Act][70015].Data;
            if (world.WorldSNO.Id == 161472)
                actData = (NullD.Common.MPQ.FileFormats.Act)MPQStorage.Data.Assets[SNOGroup.Act][70016].Data;
            if (world.WorldSNO.Id == 172909)
                actData = (NullD.Common.MPQ.FileFormats.Act)MPQStorage.Data.Assets[SNOGroup.Act][70017].Data;
            if (world.WorldSNO.Id == 178152)
                actData = (NullD.Common.MPQ.FileFormats.Act)MPQStorage.Data.Assets[SNOGroup.Act][70018].Data;
            #endregion

            #region Внутри игры
            foreach (var player in world.Game.Players.Values)
            {
                
                if (player.Toon.ActiveAct == 0)
                {
                    actData = (NullD.Common.MPQ.FileFormats.Act)MPQStorage.Data.Assets[SNOGroup.Act][70015].Data;
                }
                else if (player.Toon.ActiveAct == 100)
                {
                    actData = (NullD.Common.MPQ.FileFormats.Act)MPQStorage.Data.Assets[SNOGroup.Act][70016].Data;
                }
                else if (player.Toon.ActiveAct == 200)
                {
                    actData = (NullD.Common.MPQ.FileFormats.Act)MPQStorage.Data.Assets[SNOGroup.Act][70017].Data;
                }
                else if (player.Toon.ActiveAct == 300)
                {
                    actData = (NullD.Common.MPQ.FileFormats.Act)MPQStorage.Data.Assets[SNOGroup.Act][70018].Data;
                }
            }
            #endregion
            var wayPointInfo = actData.WayPointInfo;

            var proximity = new Rect(this.Position.X - 1.0, this.Position.Y - 1.0, 2.0, 2.0);
            var scenes = this.World.QuadTree.Query<Scene>(proximity);
            if (scenes.Count == 0) return; // TODO: fixme! /raist

            var scene = scenes[0]; // Parent scene /fasbat

            if (scenes.Count == 2) // What if it's a subscene? /fasbat
            {
                if (scenes[1].ParentChunkID != 0xFFFFFFFF)
                    scene = scenes[1];
            }

            for (int i = 0; i < wayPointInfo.Length; i++)
            {
                // World - Level
                //117405 - 117411
                //167721 - 119870
                if (wayPointInfo[i].SNOLevelArea == -1)
                    continue;

                if (scene.Specification == null) continue;
                foreach (var area in scene.Specification.SNOLevelAreas)
                {
                    if (wayPointInfo[i].SNOWorld != this.World.WorldSNO.Id || wayPointInfo[i].SNOLevelArea != area)
                        continue;

                    this.WaypointId = i;
                    break;
                }
            }

        }

        public override void OnTargeted(Player player, Net.GS.Message.Definitions.World.TargetMessage message)
        {
            var world = player.World;
            //194401


            world.BroadcastIfRevealed(new PlayAnimationMessage()
            {
                ActorID = this.DynamicID,
                Field1 = 5,
                Field2 = 0f,
                tAnim = new[]
                    {
                        new PlayAnimationMessageSpec()
                        {
                            Duration = 4,
                            AnimationSNO = 0x2f761,
                            PermutationIndex = 0,
                            Speed = 1f,
                        }
                    }
            }, this);

            this.PlayActionAnimation(194401);

            player.InGameClient.SendMessage(new ANNDataMessage(Opcodes.OpenWaypointSelectionWindowMessage)
            {
                ActorID = this.DynamicID
            });

            this.Attributes[Net.GS.Message.GameAttribute.Gizmo_Has_Been_Operated] = true;
        }

        public override bool Reveal(Player player)
        {
            if (!base.Reveal(player))
                return false;

            // Show a minimap icon
            player.InGameClient.SendMessage(new MapMarkerInfoMessage()
            {
                Field0 = (int)World.NewActorID,    // TODO What is the correct id space for mapmarkers? /fasbat
                Field1 = new WorldPlace()
                {
                    Position = this.Position,
                    WorldID = this.World.DynamicID
                },
                Field2 = 0x1FA21, // 129569 // Healer - 230136 0x382F8
                m_snoStringList = 0xF063,

                Field3 = unchecked((int)0x9799F57B),
                Field9 = 0,
                Field10 = 0,
                Field11 = 0,
                Field5 = 0,
                Field6 = true,
                Field7 = false,
                Field8 = false,
                Field12 = 0
            });

            this.World.SpawnMonster(4686, new Vector3D(Position.X, Position.Y, Position.Z - 1));
            //uint Need = World.NewActorID - 2;

            var actors = this.GetActorsInRange(1f);
            foreach (var actor in actors)
            {
                if (actor.ActorSNO.Id == 4686)
                {
                    actor.Attributes[GameAttribute.MinimapIconOverride] = 0x1FA21;
                    actor.Attributes[GameAttribute.MinimapActive] = true;
                    actor.Attributes.BroadcastChangedIfRevealed();
                }
            }

            return true;
        }
    }
}
