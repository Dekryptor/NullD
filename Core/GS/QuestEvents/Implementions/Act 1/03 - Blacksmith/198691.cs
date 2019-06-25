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
    class _198691 : QuestEvent
    {
        private static readonly Logger Logger = LogManager.CreateLogger();
        private Boolean HadConversation = true;

        public _198691()
            : base(198691)
        {
        }

        public override void Execute(Map.World world)
        {
            if (HadConversation)
            {
                HadConversation = false;

            }

            foreach (var player in world.Players.Values)
            {
                player.Toon.ActiveQuest = 72221;
                player.Toon.StepOfQuest = 1;
                player.Toon.StepIDofQuest = -1;
            }

            var BlacksmithQuest = world.GetActorBySNO(65036);
            (BlacksmithQuest as Core.GS.Actors.InteractiveNPC).Conversations.Clear();
            (BlacksmithQuest as Actors.InteractiveNPC).Conversations.Add(new Actors.Interactions.ConversationInteraction(198292));
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
