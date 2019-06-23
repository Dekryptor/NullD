using NullD.Common.Logging;
using NullD.Core.GS.Common.Types.Math;
using NullD.Core.GS.Common.Types.TagMap;
using NullD.Net.GS.Message.Definitions.Animation;
using System;

namespace NullD.Core.GS.QuestEvents.Implementations
{
    class _17667 : QuestEvent
    {

        private static readonly Logger Logger = LogManager.CreateLogger();
        private Boolean HadConversation = true;


        public _17667()
            : base(17667)
        {
        }

        public override void Execute(Map.World world)
        {
            if (HadConversation)
            {
                HadConversation = false;
                var CainBrains = world.GetActorsBySNO(102386);
                Vector3D CainPath = new Vector3D(76.99389f, 155.145f, 0.0997252f);
                var FindDynamicCain = world.GetActorByDynamicId(2297);
                foreach (var AnyCain in CainBrains)
                {
                    var facingAngle = Actors.Movement.MovementHelpers.GetFacingAngle(AnyCain, CainPath);
                    AnyCain.Move(CainPath, facingAngle);
                }

                world.Game.Quests.Advance(72095);
                Logger.Debug(" Quests.Advance(72095) ");

                Logger.Debug(" Dialog with Cain ");
            }
            foreach (var player in world.Players.Values)
            {

                player.Toon.ActiveQuest = 72095;
                player.Toon.StepOfQuest = 14;
                player.Toon.StepIDofQuest = 19;
                player.Toon.WayPointStatus = 7;
                

                Logger.Debug(" Progress Saved ");
                player.UpdateHeroState();

            };

            var BookShelf = world.GetActorBySNO(5723);
            world.BroadcastIfRevealed(new PlayAnimationMessage
            {
                ActorID = BookShelf.DynamicID,
                Field1 = 5,
                Field2 = 0,
                tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                {
                    new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                    {
                        Duration = 100,
                        AnimationSNO = BookShelf.AnimationSet.TagMapAnimDefault[AnimationSetKeys.Opening],
                        PermutationIndex = 0,
                        Speed = 1
                    }
                }
            }, BookShelf);

            world.BroadcastIfRevealed(new SetIdleAnimationMessage
            {
                ActorID = BookShelf.DynamicID,
                AnimationSNO = AnimationSetKeys.Open.ID
            }, BookShelf);

            //*/


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
