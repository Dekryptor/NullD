using NullD.Common.MPQ.FileFormats;
using NullD.Net.GS.Message.Definitions.Animation;
using NullD.Common.Logging;
using NullD.Core.GS.Actors.Implementations;
using NullD.Net.GS.Message;
using System;

namespace NullD.Core.GS.QuestEvents.Implementations
{
    class _166678 : QuestEvent
    {

        private static readonly Logger Logger = LogManager.CreateLogger();

        public _166678()
            : base(166678)
        {
        }

        public override void Execute(Map.World world)
        {
            #region Открываем ворота
            var Gate = world.GetActorBySNO(108466);
            var NoGate = new Door(world, 108466, world.GetActorBySNO(108466).Tags);
            NoGate.Field2 = 16;
            NoGate.RotationAxis = world.GetActorBySNO(108466).RotationAxis;
            NoGate.RotationW = world.GetActorBySNO(108466).RotationW;
            NoGate.Attributes[GameAttribute.Gizmo_Has_Been_Operated] = true;
            NoGate.Attributes[GameAttribute.Gizmo_State] = 1;
            NoGate.Attributes[GameAttribute.Untargetable] = true;
            NoGate.Attributes.BroadcastChangedIfRevealed();
            NoGate.EnterWorld(world.GetActorBySNO(108466).Position);
            Gate.Destroy();
            #endregion

            world.BroadcastIfRevealed(new PlayAnimationMessage
            {
                ActorID = NoGate.DynamicID,
                Field1 = 5,
                Field2 = 0,
                tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                {
                    new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                    {
                        Duration = 50,
                        AnimationSNO = NoGate.AnimationSet.TagMapAnimDefault[Core.GS.Common.Types.TagMap.AnimationSetKeys.Opening],
                        PermutationIndex = 0,
                        Speed = 1
                    }
                }
            }, NoGate);

            world.BroadcastIfRevealed(new SetIdleAnimationMessage
            {
                ActorID = NoGate.DynamicID,
                AnimationSNO = Core.GS.Common.Types.TagMap.AnimationSetKeys.Open.ID
            }, NoGate);

            foreach (var plr in world.Players.Values)
                if (plr.PlayerIndex == 0)
                    plr.ActiveHireling.Brain.Activate();

            world.Game.Quests.NotifyQuest(72095, QuestStepObjectiveType.EventReceived, -1);
        }

        private bool StartConversation(Map.World world, Int32 conversationId)
        {
            foreach (var player in world.Players)
            {
                player.Value.Conversations.StartConversation(conversationId);
            }
            return true;
        }
    }
}
