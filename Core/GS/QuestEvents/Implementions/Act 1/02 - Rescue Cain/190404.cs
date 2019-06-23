using System;
using NullD.Common.Logging;

namespace NullD.Core.GS.QuestEvents.Implementations
{
    class _190404 : QuestEvent
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        public _190404()
            : base(190404)
        {
        }

        public override void Execute(Map.World world)
        {
            Logger.Debug(" Разговор с Леей закончен ");
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