using NullD.Core.GS.Map;
using NullD.Core.GS.Common.Types.TagMap;

namespace NullD.Core.GS.Actors.Implementations
{
    [HandledSNO(203030)]
    class LeahInCellar : InteractiveNPC
    {
        public LeahInCellar(World world, int snoID, TagMap tags)
            : base(world, snoID, tags)
        {
            this.Field7 = 1;
            this.ConversationList = null;
            this.Conversations.Add(new Actors.Interactions.ConversationInteraction(198588));

        }

    }
}