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

using System.Text;
using NullD.Net.GS.Message.Fields;

namespace NullD.Net.GS.Message.Definitions.Player
{
    [Message(Opcodes.NewPlayerMessage)]
    public class NewPlayerMessage : GameMessage
    {
        public int PlayerIndex;
        public EntityId ToonId;
        public EntityId GameAccountId;
        public string ToonName;
        public int Field3;
        public int Field4;
        public int /* sno */ snoActorPortrait;
        public int Level;
        public int AltLevel;
        public HeroStateData StateData;
        public bool Field8;
        public int Field9;
        public uint ActorID; // Hero's DynamicID

        public NewPlayerMessage() : base(Opcodes.NewPlayerMessage) {}

        public override void Parse(GameBitBuffer buffer)
        {
            PlayerIndex = buffer.ReadInt(3);
            ToonId = new EntityId();
            ToonId.Parse(buffer);
            GameAccountId = new EntityId();
            GameAccountId.Parse(buffer);
            ToonName = buffer.ReadCharArray(49);
            Field3 = buffer.ReadInt(5) + (-1);
            Field4 = buffer.ReadInt(3) + (-1);
            snoActorPortrait = buffer.ReadInt(32);
            Level = buffer.ReadInt(7);
            AltLevel = buffer.ReadInt(7);
            StateData = new HeroStateData();
            StateData.Parse(buffer);
            Field8 = buffer.ReadBool();
            Field9 = buffer.ReadInt(32);
            ActorID = buffer.ReadUInt(32);
        }

        public override void Encode(GameBitBuffer buffer)
        {
            buffer.WriteInt(3, PlayerIndex);
            ToonId.Encode(buffer);
            GameAccountId.Encode(buffer);
            buffer.WriteCharArray(49, ToonName);
            buffer.WriteInt(5, Field3 - (-1));
            buffer.WriteInt(3, Field4 - (-1));
            buffer.WriteInt(32, snoActorPortrait);
            buffer.WriteInt(7, Level);
            buffer.WriteInt(7, AltLevel);
            StateData.Encode(buffer);
            buffer.WriteBool(Field8);
            buffer.WriteInt(32, Field9);
            buffer.WriteUInt(32, ActorID);
        }

        public override void AsText(StringBuilder b, int pad)
        {
            b.Append(' ', pad);
            b.AppendLine("NewPlayerMessage:");
            b.Append(' ', pad++);
            b.AppendLine("{");
            b.Append(' ', pad); b.AppendLine("PlayerIndex: 0x" + PlayerIndex.ToString("X8") + " (" + PlayerIndex + ")");
            ToonId.AsText(b, pad);
            GameAccountId.AsText(b, pad);
            b.Append(' ', pad); b.AppendLine("ToonName: \"" + ToonName + "\"");
            b.Append(' ', pad); b.AppendLine("Field3: 0x" + Field3.ToString("X8") + " (" + Field3 + ")");
            b.Append(' ', pad); b.AppendLine("Field4: 0x" + Field4.ToString("X8") + " (" + Field4 + ")");
            b.Append(' ', pad); b.AppendLine("Level: 0x" + Level.ToString("X8") + " (" + Level + ")");
            b.Append(' ', pad); b.AppendLine("AltLevel: 0x" + AltLevel.ToString("X8") + " (" + AltLevel + ")");
            StateData.AsText(b, pad);
            b.Append(' ', pad); b.AppendLine("Field8: " + (Field8 ? "true" : "false"));
            b.Append(' ', pad); b.AppendLine("Field9: 0x" + Field9.ToString("X8") + " (" + Field9 + ")");
            b.Append(' ', pad); b.AppendLine("ActorID: 0x" + ActorID.ToString("X8") + " (" + ActorID + ")");
            b.Append(' ', --pad);
            b.AppendLine("}");
        }


    }
}
