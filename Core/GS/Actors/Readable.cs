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

namespace NullD.Core.GS.Actors.Implementations
{
    /// <summary>
    /// Class that implements healthwell, run power on click and change gizmo state
    /// </summary>
    class Readable : Gizmo
    {
        public TagMap Tags;
        public int Tom;

        public Readable(World world, int snoId, TagMap tags)
            : base(world, snoId, tags)
        {
            Attributes[GameAttribute.Gizmo_State] = 0;
            Field2 = 0;
            Tags = tags;

            if (world.GetActorsBySNO(snoId) == null)
                Tom = 0;
            else
            {
                Tom = world.GetActorsBySNO(snoId).Count;
            }
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
            NullD.Common.MPQ.FileFormats.TreasureClass Treasure = null;
            if (ActorData.TagMap.ContainsKey(ActorKeys.LootTreasureClass))
                Treasure = (NullD.Common.MPQ.FileFormats.TreasureClass)ActorData.TagMap[ActorKeys.LootTreasureClass].Target;

            if (Treasure != null)
            {
                System.Collections.Generic.List<int> GBids = new System.Collections.Generic.List<int> { };
                int LoreSNOId = 0;

                string RawLores = player.Toon.LoreCollected;
                System.Collections.Generic.List<int> Lores = new System.Collections.Generic.List<int> { };


                foreach (var Loot in Treasure.LootDropModifiers)
                {
                    if (Loot.ItemSpecifier.ItemGBId != 0)
                    {
                        var ItemDef = Items.ItemGenerator.GetItemDefinition(Loot.ItemSpecifier.ItemGBId);
                        var item = Items.ItemGenerator.CookFromDefinition(player, ItemDef);
                        LoreSNOId = item.ActorData.TagMap[ActorKeys.Lore].Id;

                        item.Destroy();

                        this.Attributes[GameAttribute.Gizmo_State] = 1;
                        this.Attributes[GameAttribute.Untargetable] = true;
                        this.Attributes.BroadcastChangedIfRevealed();
                        if (!player.HasLore(LoreSNOId))
                        {
                            player.PlayLore(LoreSNOId, true);
                            player.UpdateExp(300);
                            Logger.Info("Book Implementaion ver 1.0, Получено опыта - 500, Book ID - {0}, Player - {1}", LoreSNOId, player.Toon.Name);
                            break;
                        }


                    }
                }





                //this.Destroy();
            }
            else if (this.ActorSNO.Id == 230232)
            {
                if (!player.HasLore(211567))
                {
                    player.PlayLore(211567, true);
                    player.UpdateExp(500);
                    Logger.Info("Book Implementaion ver 1.0, Получено опыта - 500, Book ID - 211567, Player - {0}", player.Toon.Name);
                }
                this.Attributes[GameAttribute.Gizmo_State] = 1;
                this.Attributes[GameAttribute.Untargetable] = true;
                this.Attributes.BroadcastChangedIfRevealed();

            }
        }
    }
}
