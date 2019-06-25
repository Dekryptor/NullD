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
using NullD.Common.Helpers.Math;
using NullD.Core.GS.Actors;

namespace NullD.Core.GS.QuestEvents.Implementations
{
    class _196041 : QuestEvent
    {
        private static readonly Logger Logger = LogManager.CreateLogger();
        private Boolean HadConversation = true;

        public _196041()
            : base(196041)
        {
        }

        public override void Execute(Map.World world)
        {
            if (HadConversation)
            {
                HadConversation = false;
                Logger.Debug(" Quests.Advance(72221) ");

                //world.Game.Quests.NotifyQuest(72221, QuestStepObjectiveType.PossessItem, -1);
                world.Game.Quests.Advance(72221);
            }
            foreach (var player in world.Players.Values)
            {
                player.Toon.MaximumQuest = 72061;
                player.Toon.ActiveQuest = 72061;
                player.Toon.StepOfQuest = 0;
                Logger.Debug(" Progress Saved ");
            };

            foreach (var plr in world.Game.Players.Values)
            {
                var inventory = plr.Inventory;
                var dbArtisans = DBSessions.AccountSession.Get<DBArtisansOfToon>(plr.Toon.PersistentID);

                plr.InGameClient.SendMessage(plr.GetBlacksmithData(dbArtisans));

                var Blacksmith = world.GetActorBySNO(56947);

                Blacksmith.Attributes[Net.GS.Message.GameAttribute.MinimapActive] = true;
                (Blacksmith as Core.GS.Actors.InteractiveNPC).Conversations.Clear();
                
                Blacksmith.Attributes[Net.GS.Message.GameAttribute.Conversation_Icon, 0] = 0;
                Blacksmith.Attributes.BroadcastChangedIfRevealed();

                foreach (var itm in inventory.GetBackPackItems())
                {
                    if (itm.ActorSNO.Id == 92168)
                    {
                        inventory.DestroyInventoryItem(itm);
                        inventory.RefreshInventoryToClient();

                        var RepairedCrown = Items.ItemGenerator.Cook(plr, "SkeletonKingCrown");
                        inventory.PickUp(RepairedCrown);
                    }
                }
                DBSessions.AccountSession.SaveOrUpdate(dbArtisans);
                DBSessions.AccountSession.Flush();
            }

            Logger.Debug(" Третий квест окончен. ");

            Logger.Debug("ПОЛУЧЕНИЕ НАГРАДЫ!");
            foreach (var player in world.Game.Players.Values)
            {
                D3.Quests.QuestReward.Builder Reward = new D3.Quests.QuestReward.Builder();

                //StoneOfRecallSkellet.Set
                var Hand = D3.GameBalance.Handle.CreateBuilder()
                    .SetGameBalanceType((int)GBHandleType.Gizmo)
                    .SetGbid(-2007738575)
               //     .SetGbid(1612257705)
                    ;
                var RareName = D3.Items.RareItemName.CreateBuilder()
                    .SetSnoAffixStringList(213647)
                    .SetAffixStringListIndex(213668)
                    .SetItemNameIsPrefix(true)
                    .SetItemStringListIndex(213647)
                    ;

                var StoneOfRecallScript = D3.Items.Generator.CreateBuilder()
                    .SetSeed((uint)RandomHelper.Next())
                    .SetGbHandle(Hand)
                    .SetDurability(1)
                    .SetMaxDurability(1)
                    .SetStackSize(1)
                    .SetFlags(0x1)
                //    .SetItemQualityLevel(6)
                //    .SetMaxDurability(30)
                //    .SetDurability(30)
                //    .SetRareItemName(RareName)
                    ;


                Reward.SnoQuest = 72221;
                Reward.GoldGranted = 195;
                Reward.XpGranted = 900;
                Reward.ItemGranted = StoneOfRecallScript.Build(); //190617
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
            //StartConversation(world, 196043);

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
