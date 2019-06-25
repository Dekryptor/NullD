using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NullD.Common.MPQ.FileFormats;
using NullD.Net.GS.Message.Definitions.ACD;
using NullD.Net.GS.Message.Definitions.Animation;
using NullD.Core.GS.Common.Types.Math;
using NullD.Core.GS.Generators;
using NullD.Common.Logging;
using System.Threading.Tasks;
using System.Threading;
using NullD.Core.GS.Map;
using NullD.Common.Storage;
using NullD.Common.Storage.AccountDataBase.Entities;

namespace NullD.Core.GS.QuestEvents.Implementations
{
    class _198292 : QuestEvent
    {
        private static readonly Logger Logger = LogManager.CreateLogger();
        private Boolean HadConversation = true;

        public _198292() 
            : base(198292)
        {
        }

        public override void Execute(Map.World world)
        {
            if (HadConversation)
            {
                HadConversation = false;
                Logger.Debug(" Quests.Advance(72221) ");
            }
            foreach (var player in world.Players.Values)
            {
                player.Toon.ActiveQuest = 72221;
                player.Toon.StepOfQuest = 2;
                Logger.Debug(" Progress Saved ");
            };
            var TELEGAS = world.GetActorsBySNO(112131);
            Vector3D LastTelega = new Vector3D();
            foreach (var TELEGA in TELEGAS)
            {
                TELEGA.Destroy();
                LastTelega = TELEGA.Position;
            }

            world.Game.Quests.NotifyQuest(72221, QuestStepObjectiveType.HadConversation, 198292);
            var BlacksmithQuest = world.GetActorBySNO(65036);
            (BlacksmithQuest as Core.GS.Actors.InteractiveNPC).Conversations.Clear();
            (BlacksmithQuest as Core.GS.Actors.InteractiveNPC).Attributes[Net.GS.Message.GameAttribute.Conversation_Icon, 0] = 0;
            (BlacksmithQuest as Core.GS.Actors.InteractiveNPC).Attributes.BroadcastChangedIfRevealed();

            BlacksmithQuest.WalkSpeed = 0.33f;
            BlacksmithQuest.RunSpeed = 0.33f;

            Vector3D FirstPoint = new Vector3D(2905.856f, 2584.807f, 0.5997877f);
            Vector3D SecondPoint = new Vector3D(2790.396f, 2611.313f, 0.5997864f);

            var FirstfacingAngle = Actors.Movement.MovementHelpers.GetFacingAngle(BlacksmithQuest, FirstPoint);

            var SecondfacingAngle = Actors.Movement.MovementHelpers.GetFacingAngle(BlacksmithQuest, SecondPoint);

            BlacksmithQuest.Move(FirstPoint, FirstfacingAngle);

            Ticker.TickTimer Timeout = new Ticker.SecondsTickTimer(world.Game, 3f);
            var ListenerKingSkeletons = System.Threading.Tasks.Task<bool>.Factory.StartNew(() => WaitToSpawn(Timeout));
            ListenerKingSkeletons.ContinueWith(delegate
            {
                BlacksmithQuest.Move(SecondPoint, SecondfacingAngle);
            });

        }
        private bool WaitToSpawn(Ticker.TickTimer timer)
        {
            while (timer.TimedOut != true)
            {

            }
            return true;
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
