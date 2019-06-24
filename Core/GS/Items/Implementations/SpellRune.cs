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

using System.Linq;
using NullD.Common.Helpers.Math;
using NullD.Common.Logging;
using NullD.Core.LogNet.Toons;
using NullD.Net.GS.Message;
using NullD.Core.GS.Map;

namespace NullD.Core.GS.Items.Implementations
{
    //[HandledType("SpellRune")]
    public class SpellRune : Item
    {
        // type of rune is in Name
        // Attributes[GameAttribute.Rune_<x>] = <rank>; // on attuned runes ONLY
        // Attributes[GameAttribute.Rune_Rank] = <in spec>; // on unattuned rune ONLY, inititalized in creation
        // Attributes[GameAttribute.Rune_Attuned_Power] = 0; // need s to be 0 on unattuned or random value from all powers

        public static readonly Logger Logger = LogManager.CreateLogger();

        public SpellRune(GS.Map.World world, NullD.Common.MPQ.FileFormats.ItemTable definition, bool Craft = false)
            : base(world, definition)
        {
            if (!definition.Name.Contains("X"))
            {
                // attuned rune, randomize power
                int classRnd = RandomHelper.Next(0, 5);
                int PowerSNOId = -1;
                switch (classRnd)
                {
                    case 0:
                        PowerSNOId = Skills.Skills.Barbarian.AllActiveSkillsList.ElementAt(RandomHelper.Next(0, NullD.Core.GS.Skills.Skills.Barbarian.AllActiveSkillsList.Count));
                        break;
                    case 1:
                        PowerSNOId = Skills.Skills.DemonHunter.AllActiveSkillsList.ElementAt(RandomHelper.Next(0, NullD.Core.GS.Skills.Skills.DemonHunter.AllActiveSkillsList.Count));
                        break;
                    case 2:
                        PowerSNOId = Skills.Skills.Monk.AllActiveSkillsList.ElementAt(RandomHelper.Next(0, NullD.Core.GS.Skills.Skills.Monk.AllActiveSkillsList.Count));
                        break;
                    case 3:
                        PowerSNOId = Skills.Skills.WitchDoctor.AllActiveSkillsList.ElementAt(RandomHelper.Next(0, NullD.Core.GS.Skills.Skills.WitchDoctor.AllActiveSkillsList.Count));
                        break;
                    case 4:
                        PowerSNOId = Skills.Skills.Wizard.AllActiveSkillsList.ElementAt(RandomHelper.Next(0, NullD.Core.GS.Skills.Skills.Wizard.AllActiveSkillsList.Count));
                        break;
                }
                //this.Attributes[GameAttribute.Rune_Attuned_Power] = PowerSNOId;
            }
        }

        /// <summary>
        /// Re-attunes rune to player's class. Used for favoring.
        /// </summary>
        /// <param name="toonClass"></param>
        public void ReAttuneToClass(ToonClass toonClass)
        {
            int PowerSNOId = -1;
            switch (toonClass)
            {
                case ToonClass.Barbarian:
                    PowerSNOId = NullD.Core.GS.Skills.Skills.Barbarian.AllActiveSkillsList.ElementAt(RandomHelper.Next(0, NullD.Core.GS.Skills.Skills.Barbarian.AllActiveSkillsList.Count));
                    break;
                case ToonClass.DemonHunter:
                    PowerSNOId = NullD.Core.GS.Skills.Skills.DemonHunter.AllActiveSkillsList.ElementAt(RandomHelper.Next(0, NullD.Core.GS.Skills.Skills.DemonHunter.AllActiveSkillsList.Count));
                    break;
                case ToonClass.Monk:
                    PowerSNOId = NullD.Core.GS.Skills.Skills.Monk.AllActiveSkillsList.ElementAt(RandomHelper.Next(0, NullD.Core.GS.Skills.Skills.Monk.AllActiveSkillsList.Count));
                    break;
                case ToonClass.WitchDoctor:
                    PowerSNOId = NullD.Core.GS.Skills.Skills.WitchDoctor.AllActiveSkillsList.ElementAt(RandomHelper.Next(0, NullD.Core.GS.Skills.Skills.WitchDoctor.AllActiveSkillsList.Count));
                    break;
                case ToonClass.Wizard:
                    PowerSNOId = NullD.Core.GS.Skills.Skills.Wizard.AllActiveSkillsList.ElementAt(RandomHelper.Next(0, NullD.Core.GS.Skills.Skills.Wizard.AllActiveSkillsList.Count));
                    break;
            }
            //this.Attributes[GameAttribute.Rune_Attuned_Power] = PowerSNOId;
        }
    }
}
