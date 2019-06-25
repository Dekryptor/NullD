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
    class _196043 : QuestEvent
    {
        private static readonly Logger Logger = LogManager.CreateLogger();
        private Boolean HadConversation = true;

        public _196043()
            : base(196043)
        {
        }

        public override void Execute(Map.World world)
        {
            if (HadConversation)
            {
                HadConversation = false;
                Logger.Debug(" Quests.Advance(72221) ");

                world.Game.Quests.Advance(72221);
            }
            foreach (var player in world.Players.Values)
            {
                player.Toon.MaximumQuest = 72061;
                player.Toon.ActiveQuest = 72061;
                player.Toon.StepOfQuest = 0;
                Logger.Debug(" Progress Saved ");
            };

            Logger.Debug(" Третий квест окончен. ");

            Logger.Debug("ПОЛУЧЕНИЕ НАГРАДЫ!");
            foreach (var player in world.Game.Players.Values)
            {
                D3.Quests.QuestReward.Builder Reward = new D3.Quests.QuestReward.Builder();
                Reward.SnoQuest = 72221;
                Reward.GoldGranted = 195;
                Reward.XpGranted = 900;

                player.Toon.StoneOfPortal = true;
                player.EnableStoneOfRecall();

                D3.Quests.QuestStepComplete.Builder StepCompleted = new D3.Quests.QuestStepComplete.Builder();
                StepCompleted.Reward = Reward.Build();
                StepCompleted.SetIsQuestComplete(true);

                player.InGameClient.SendMessage(new Net.GS.Message.Definitions.Quest.QuestStepCompleteMessage()
                {
                    QuestStepComplete = StepCompleted.Build()
                });
                player.Inventory.AddGoldAmount(Reward.GoldGranted);
                player.UpdateExp(Reward.XpGranted);
            }
            bool questConversation = true;
            var Cain = world.GetActorBySNO(3533);
            Cain.Attributes[Net.GS.Message.GameAttribute.MinimapActive] = true;
            (Cain as Actors.InteractiveNPC).Conversations.Clear();
            (Cain as Actors.InteractiveNPC).Conversations.Add(new Actors.Interactions.ConversationInteraction(80681));

            Cain.Attributes[Net.GS.Message.GameAttribute.Conversation_Icon, 0] = questConversation ? 1 : 0;
            Cain.Attributes.BroadcastChangedIfRevealed();
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
