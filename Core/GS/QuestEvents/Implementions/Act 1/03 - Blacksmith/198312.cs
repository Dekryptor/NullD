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
    class _198312 : QuestEvent
    {
        private static readonly Logger Logger = LogManager.CreateLogger();
        private Boolean HadConversation = true;

        public _198312()
            : base(198312)
        {
        }

        public override void Execute(Map.World world)
        {
            if (HadConversation)
            {
                HadConversation = false;
                world.Game.Quests.NotifyQuest(72221, QuestStepObjectiveType.EventReceived, -1);
                Logger.Debug(" Quests.NotifyQuest(72221) ");
            }
            foreach (var player in world.Players.Values)
            {
                player.Toon.ActiveQuest = 72221;
                player.Toon.StepIDofQuest = 35;
                player.Toon.StepOfQuest = 5;
                Logger.Debug(" Progress Saved ");

            };
            var BlacksmithQuest = world.GetActorBySNO(65036);
            (BlacksmithQuest as Core.GS.Actors.InteractiveNPC).Conversations.Clear();
            (BlacksmithQuest as Core.GS.Actors.InteractiveNPC).Attributes[Net.GS.Message.GameAttribute.Conversation_Icon, 0] = 0;
            (BlacksmithQuest as Core.GS.Actors.InteractiveNPC).Attributes.BroadcastChangedIfRevealed();
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
//196041