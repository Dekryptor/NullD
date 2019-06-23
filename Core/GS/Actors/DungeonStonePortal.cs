using NullD.Common.Helpers.Hash;
using NullD.Common.Logging;
using NullD.Core.GS.Common.Types.TagMap;
using NullD.Core.GS.Map;
using NullD.Core.GS.Players;
using NullD.Net.GS.Message;
using NullD.Net.GS.Message.Definitions.Map;
using NullD.Net.GS.Message.Definitions.Misc;
using NullD.Net.GS.Message.Definitions.Pet;
using NullD.Net.GS.Message.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullD.Core.GS.Actors
{
    [HandledSNO(135248)]//
    class DungeonStonePortal : Actor
    {
        static readonly Logger Logger = LogManager.CreateLogger();

        public override ActorType ActorType { get { return ActorType.Gizmo; } }
        public ResolvedPortalDestination Destination { get; set; }
        public NullD.Common.MPQ.FileFormats.Scene.NavZoneDef NavZone { get; private set; }
        public bool Activated = true;
        private int MinimapIcon;

        public DungeonStonePortal(World world, int snoId, TagMap tags)
            : base(world, snoId, tags)
        {
            try
            {
                if (world.WorldSNO.Id == 60600)
                {
                    this.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = 71150,
                        DestLevelAreaSNO = 72712,
                        StartingPointActorTag = -606
                    };
                }
                else if (world.WorldSNO.Id == 72636)
                {
                    this.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = 71150,
                        DestLevelAreaSNO = 72712,
                        StartingPointActorTag = -636
                    };
                }
                else if (world.WorldSNO.Id == 72637)
                {
                    this.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = 71150,
                        DestLevelAreaSNO = 72712,
                        StartingPointActorTag = -637
                    };
                }
                else if (world.WorldSNO.Id == 211471)
                {
                    this.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = 71150,
                        DestLevelAreaSNO = 91133,
                        StartingPointActorTag = -471
                    };
                }
                else
                {
                    this.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = tags[MarkerKeys.DestinationWorld].Id,
                        DestLevelAreaSNO = tags[MarkerKeys.DestinationLevelArea].Id,
                        StartingPointActorTag = tags[MarkerKeys.DestinationActorTag]
                    };

                    // Override minimap icon in merkerset tags
                    if (tags.ContainsKey(MarkerKeys.MinimapTexture))
                    {
                        MinimapIcon = tags[MarkerKeys.MinimapTexture].Id;
                    }
                    else
                    {
                        MinimapIcon = ActorData.TagMap[ActorKeys.MinimapMarker].Id;
                    }
                }
            }
            catch (KeyNotFoundException)
            {
                this.Destination = new ResolvedPortalDestination
                {
                    WorldSNO = -1,
                    DestLevelAreaSNO = -1,
                    StartingPointActorTag = -1
                };
            }
            Field2 = 0;

            this.Attributes[Net.GS.Message.GameAttribute.MinimapActive] = true;
            //MinimapIcon
            this.Attributes[GameAttribute.MinimapActive] = true;
        }

        public override bool Reveal(Player player)
        {
            //Logger.Debug(" (Reveal) portal {0} has location {1}", this.ActorSNO, this._position);
            if (!base.Reveal(player) || this.Destination == null)
                return false;

            if (!Activated)
                return false;

            player.InGameClient.SendMessage(new PortalSpecifierMessage()
            {
                ActorID = this.DynamicID,
                Destination = this.Destination
            });

            // Show a minimap icon
            NullD.Common.MPQ.Asset asset;
            string markerName = "";

            if (NullD.Common.MPQ.MPQStorage.Data.Assets[Common.Types.SNO.SNOGroup.LevelArea].TryGetValue(this.Destination.DestLevelAreaSNO, out asset))
                markerName = System.IO.Path.GetFileNameWithoutExtension(asset.FileName);

            player.InGameClient.SendMessage(new MapMarkerInfoMessage()
            {
                Field0 = (int)World.NewSceneID,    // TODO What is the correct id space for mapmarkers?
                Field1 = new WorldPlace()
                {
                    Position = this.Position,
                    WorldID = this.World.DynamicID
                },
                Field2 = 0x00018FB0,  /* Marker_DungeonEntrance.tex */          // TODO Dont mark all portals as dungeon entrances... some may be exits too (although d3 does not necesarrily use the correct markers). Also i have found no hacky way to determine whether a portal is entrance or exit - farmy
                m_snoStringList = 0x0000CB2E, /* LevelAreaNames.stl */          // TODO Dont use hardcoded numbers

                Field3 = StringHashHelper.HashNormal(markerName),
                Field9 = 0,
                Field10 = 0,
                Field11 = 0,
                Field5 = 0,
                Field6 = true,
                Field7 = false,
                Field8 = true,
                Field12 = 0
            });

            if (this.ActorSNO.Id != 5648 & this.ActorSNO.Id != 5660)// && this.ActorSNO.Id != 229013)
            {
                //this.World.SpawnMonster(4686, new Vector3D(Position.X, Position.Y, Position.Z - 1));
                //uint Need = World.NewActorID - 2;

                var actors = this.GetActorsInRange(2f);
                foreach (var actor in actors)
                {
                    if (actor.ActorSNO.Id == 4686)
                    {

                        if (this.ActorSNO.Id != 5648 && this.ActorSNO.Id != 229013)
                        {
                            actor.Attributes[GameAttribute.MinimapIconOverride] = this.ActorData.TagMap[ActorKeys.MinimapMarker].Id;
                        }
                        else
                        {
                            actor.Attributes[GameAttribute.MinimapIconOverride] = 102321;
                        }
                        actor.Attributes[GameAttribute.MinimapActive] = true;
                        actor.Attributes.BroadcastChangedIfRevealed();
                    }
                }
            }
            return true;
        }

        public override void OnTargeted(Players.Player player, Net.GS.Message.Definitions.World.TargetMessage message)
        {
            Logger.Debug("(OnTargeted) Portal has been activated ");

            var world = this.World.Game.GetWorld(this.Destination.WorldSNO);
            var now_world = player.World;

            if (world == null)
            {
                Logger.Warn("Portal's destination world does not exist (WorldSNO = {0})", this.Destination.WorldSNO);
                return;
            }

            var startingPoint = world.GetStartingPointById(this.Destination.StartingPointActorTag);

            if (startingPoint != null)
            {
                player.ChangeWorld(world, startingPoint);
            }
            else if (this.Destination.StartingPointActorTag == -606)
            {
                player.ChangeWorld(world, new Common.Types.Math.Vector3D(2233.019f, 1801.747f, 5.950454f));
            }
            else if (this.Destination.StartingPointActorTag == -636)
            {
                player.ChangeWorld(world, new Common.Types.Math.Vector3D(2037.89f, 1775.57f, 0f));
            }
            else if (this.Destination.StartingPointActorTag == -637)
            {
                player.ChangeWorld(world, new Common.Types.Math.Vector3D(2177f, 1946f, -4.968689f));
            }
            else if (this.Destination.StartingPointActorTag == -471)
            {
                player.ChangeWorld(world, new Common.Types.Math.Vector3D(2201.044f, 2531.148f, -27.36831f));
            }
            else
            {
                Logger.Warn("Portal's tagged starting point does not exist (Tag = {0})", this.Destination.StartingPointActorTag);
            }

            #region Тестовая функция: переход наёмника
            if (player.ActiveHireling != null)
            {
                player.ActiveHireling.ChangeWorld(world, startingPoint);

                player.InGameClient.SendMessage(new PetMessage()
                {
                    Field0 = 0,
                    Field1 = 0,
                    PetId = player.ActiveHireling.DynamicID,
                    Field3 = 0,
                });

            }
            #endregion
            //base.OnTargeted(player, message);
        }
    }
}
