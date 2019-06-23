using NullD.Core.GS.Common.Types.TagMap;
using NullD.Core.GS.Map;
using NullD.Net.GS.Message;
using NullD.Net.GS.Message.Definitions.Animation;
using NullD.Net.GS.Message.Definitions.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullD.Core.GS.Actors
{
    class Desctructible : Gizmo
    {

        public Desctructible(World world, int snoId, TagMap tags)
            : base(world, snoId, tags)
        {
            Field2 = 0x8;
            this.GBHandle.Type = (int)GBHandleType.Monster; this.GBHandle.GBID = 1;
        }

        public void ReceiveDamage(Actor source, float damage /* critical, type */)
        {
            World.BroadcastIfRevealed(new FloatingNumberMessage
            {
                Number = damage,
                ActorID = this.DynamicID,
                Type = FloatingNumberMessage.FloatType.White
            }, this);


            Attributes[GameAttribute.Hitpoints_Cur] = Math.Max(Attributes[GameAttribute.Hitpoints_Cur] - damage, 0);
            Attributes[GameAttribute.Last_Damage_ACD] = unchecked((int)source.DynamicID);

            Attributes.BroadcastChangedIfRevealed();

            if (Attributes[GameAttribute.Hitpoints_Cur] == 0)
            {
                Die();
            }
        }

        public void Die()
        {
            World.BroadcastIfRevealed(new PlayAnimationMessage
            {
                ActorID = this.DynamicID,
                Field1 = 11,
                Field2 = 0,
                tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                {
                    new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                    {
                        Duration = 10,
                        AnimationSNO = AnimationSet.TagMapAnimDefault[AnimationSetKeys.DeathDefault],
                        PermutationIndex = 0,
                        Speed = 1
                    }
                }

            }, this);

            this.Attributes[GameAttribute.Deleted_On_Server] = true;
            this.Attributes[GameAttribute.Could_Have_Ragdolled] = true;
            Attributes.BroadcastChangedIfRevealed();
            this.Destroy();
        }


        public override void OnTargeted(Players.Player player, Net.GS.Message.Definitions.World.TargetMessage message)
        {
            ReceiveDamage(player, 100);
        }
    }
}
