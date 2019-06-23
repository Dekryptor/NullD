using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullD.Net.GS.Message.Definitions.Misc
{
    [Message(new[] { Opcodes.EquipHealthPotion }, Consumers.Player)]
    public class EquipHealthPotion : GameMessage
    {
        public int /* gbid */ Field0;

        public override void Parse(GameBitBuffer buffer)
        {
            Field0 = buffer.ReadInt(32);
        }

        public override void Encode(GameBitBuffer buffer)
        {
            buffer.WriteInt(32, Field0);
        }

        public override void AsText(StringBuilder b, int pad)
        {
            b.Append(' ', pad);
            b.AppendLine("GBIDDataMessage:");
            b.Append(' ', pad++);
            b.AppendLine("{");
            b.Append(' ', pad); b.AppendLine("Field0: 0x" + Field0.ToString("X8"));
            b.Append(' ', --pad);
            b.AppendLine("}");
        }


    }
}
