﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NullD.Core.GS.Powers;
using NullD.Core.GS.AI.Brains;
using NullD.Net.GS.Message;
using NullD.Core.GS.Ticker;
using NullD.Core.GS.Players;
using NullD.Net.GS.Message.Definitions.Pet;

namespace NullD.Core.GS.Actors.Implementations.Minions
{
    class AncientMawdawc : Minion
    {
        public AncientMawdawc(Map.World world, PowerContext context, int AncientsID)
            : base(world, 90536, context.User, null)
        {
            Scale = 1.2f; //they look cooler bigger :)
            //TODO: get a proper value for this.
            this.WalkSpeed *= 5;
            SetBrain(new MinionBrain(this));
            (Brain as MinionBrain).AddPresetPower(30592);  //Weapon_Instant
            (Brain as MinionBrain).AddPresetPower(187092); //basic melee
            (Brain as MinionBrain).AddPresetPower(168827); //Seismic Slam //Only Active with Rune_C
            (Brain as MinionBrain).AddPresetPower(168828); //Weapon Throw
            //TODO: These values should most likely scale, but we don't know how yet, so just temporary values.
            Attributes[GameAttribute.Hitpoints_Max] = 20f;
            Attributes[GameAttribute.Hitpoints_Cur] = 20f;
            Attributes[GameAttribute.Attacks_Per_Second] = 1.0f;

            Attributes[GameAttribute.Damage_Weapon_Min, 0] = context.ScriptFormula(11) * context.User.Attributes[GameAttribute.Damage_Weapon_Min_Total, 0];
            Attributes[GameAttribute.Damage_Weapon_Delta, 0] = context.ScriptFormula(13) * context.User.Attributes[GameAttribute.Damage_Weapon_Delta_Total, 0];

            Attributes[GameAttribute.Pet_Type] = 0x8;
            //Pet_Owner and Pet_Creator seems to be 0
            (context.User as Player).InGameClient.SendMessage(new PetMessage()
            {
                Field0 = 0,
                Field1 = AncientsID,
                PetId = this.DynamicID,
                Field3 = 0x8,
            });
        }
    }
}
