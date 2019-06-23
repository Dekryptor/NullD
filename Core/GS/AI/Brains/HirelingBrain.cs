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

using NullD.Common.Helpers.Math;
using NullD.Common.MPQ;
using NullD.Core.GS.Actors;
using NullD.Core.GS.Actors.Actions;
using NullD.Core.GS.Actors.Implementations.Hirelings;
using NullD.Core.GS.Actors.Movement;
using NullD.Core.GS.Common.Types.Math;
using NullD.Core.GS.Common.Types.SNO;
using NullD.Core.GS.Players;
using NullD.Core.GS.Ticker;
using NullD.Net.GS.Message;
using System.Collections.Generic;

namespace NullD.Core.GS.AI.Brains
{
    public class HirelingBrain : Brain
    {
        // list of power SNOs that are defined for the monster
        public List<int> PresetPowers { get; private set; }

        private TickTimer _powerDelay;
        private Actor _target { get; set; }
        public Player Owner { get; private set; }

        public HirelingBrain(Actor body)
            : base(body)
        {
            this.PresetPowers = new List<int>();

            // build list of powers defined in monster mpq data
            if (body.ActorData.MonsterSNO > 0)
            {
                var monsterData = (NullD.Common.MPQ.FileFormats.Monster)MPQStorage.Data.Assets[SNOGroup.Monster][body.ActorData.MonsterSNO].Data;
                //SkillKit - Scoundrel - 35582

                foreach (var monsterSkill in monsterData.SkillDeclarations)
                {
                    if (monsterSkill.SNOPower > 0)
                    {
                        this.PresetPowers.Add(monsterSkill.SNOPower);
                    }
                }
            }
        }

        public override void Think(int tickCounter)
        {
            // this needed? /mdz
            //if (this.Body is NPC) return;

            // check if in disabled state, if so cancel any action then do nothing
            if (this.Body.Attributes[GameAttribute.Frozen] ||
                this.Body.Attributes[GameAttribute.Stunned] ||
                this.Body.Attributes[GameAttribute.Blind] ||
                this.Body.World.BuffManager.GetFirstBuff<Powers.Implementations.KnockbackBuff>(this.Body) != null)
            {
                if (this.CurrentAction != null)
                {
                    this.CurrentAction.Cancel(tickCounter);
                    this.CurrentAction = null;
                }
                _powerDelay = null;

                return;
            }

            // select and start executing a power if no active action
            if (this.CurrentAction == null)
            {
                // do a little delay so groups of monsters don't all execute at once
                if (_powerDelay == null)
                    _powerDelay = new SecondsTickTimer(this.Body.World.Game, (float)RandomHelper.NextDouble());

                if (_powerDelay.TimedOut)
                {
                    if (this.Body.GetObjectsInRange<Monster>(20f).Count != 0)
                    {
                        _target = this.Body.GetObjectsInRange<Monster>(40f)[0];
                        //System.Console.Out.WriteLine("Enemy in range, use powers");
                        //This will only attack when you and your minions are not moving..TODO: FIX.
                        int powerToUse = PickPowerToUse();
                        if (powerToUse > 0)
                            this.CurrentAction = new PowerAction(this.Body, powerToUse, _target);
                    }
                    else
                    {
                        //System.Console.Out.WriteLine("No enemies in range, return to master");
                        //TODO: Minions need to be behind Toons on either side. 1st Master 2nd 3rd
                        Vector3D ModdedPosition = new Vector3D((this.Body as Hireling).Master.Position.X + 5, (this.Body as Hireling).Master.Position.Y, (this.Body as Hireling).Master.Position.Z);
                        this.CurrentAction = new MoveToPointWithPathfindAction(this.Body, ModdedPosition);
                    }
                }
            }
        }

        protected virtual int PickPowerToUse()
        {
            // randomly used an implemented power
            if (this.PresetPowers.Count > 0)
            {
                int powerIndex = RandomHelper.Next(this.PresetPowers.Count);
                if (Body is Templar)
                {
                    if (PresetPowers[powerIndex] == 1747)
                    {
                        int luckyHeal = RandomHelper.Next(1, 10);
                        if (luckyHeal < 8)
                            powerIndex = 0;
                    }
                }
                else if (Body is Enchantress)
                {
                    if (Body.Attributes[GameAttribute.Skill, 101461] == 0)
                    {
                        foreach (var power in this.PresetPowers)
                            if (power == 101461)
                                return 101461;
                    }

                    if (PresetPowers[powerIndex] == 102133) // Отражение
                    {
                        if (RandomHelper.Next(1, 10) < 8)
                            powerIndex = 0;
                    }
                    else if (PresetPowers[powerIndex] == 102057) //Очарование
                    {
                        if (RandomHelper.Next(1, 10) < 7)
                            powerIndex = 0;
                    }
                }
                else if (Body is Scoundrel)
                {

                }
                if (Powers.PowerLoader.HasImplementationForPowerSNO(this.PresetPowers[powerIndex]))
                    return this.PresetPowers[powerIndex];
            }

            // no usable power
            return -1;
        }

        public void AddPresetPower(int powerSNO)
        {
            this.PresetPowers.Add(powerSNO);
        }
    }
}
