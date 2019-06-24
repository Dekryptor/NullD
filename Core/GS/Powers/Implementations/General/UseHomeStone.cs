using System.Collections.Generic;
using NullD.Core.GS.Common.Types.Math;
using NullD.Core.GS.Players;
using NullD.Core.GS.Ticker;
using NullD.Core.GS.Actors;
using NullD.Core.GS.Common.Types.TagMap;

namespace NullD.Core.GS.Powers.Implementations.General
{
    class UseHomeStone
    {
        [ImplementsPowerSNO(191590)]
        public class UseStoneOfRecall : Skill
        {
            public override IEnumerable<TickTimer> Main()
            {
                Logger.Debug("Portal to New Tristram. Version 3.0");
                int TargetWorld = -1;
                int TargetArea = -1;
                //Очистка от существующих порталов домой.
                var OldOTG = User.World.GetActorsBySNO(5648);
                if (OldOTG != null)
                    foreach (var OldP in OldOTG)
                        OldP.Destroy();

                switch ((User as Player).Toon.ActiveAct)
                {
                    case 0:
                        TargetWorld = 71150;
                        TargetArea = -1;
                        break;
                    case 100:
                        TargetWorld = 161472;
                        TargetArea = -1;
                        break;
                    case 200:
                        TargetWorld = 172909;
                        TargetArea = -1;
                        break;
                    case 300:
                        TargetWorld = 178152;
                        TargetArea = -1;
                        break;
                }
                TagMap New = new TagMap();

                New.Add(new TagKeySNO(526850), new TagMapEntry(526850, TargetWorld, 0));
                New.Add(new TagKeySNO(526853), new TagMapEntry(526853, TargetArea, 0));

                var ToHome = new Portal(User.World, 5648, New);// User.World.Game.GetWorld(TargetHome).StartingPoints[0].Tags);



                ToHome.Scale = 0.9f;
                Vector3D PositionToPortal = new Vector3D(User.Position.X, User.Position.Y + 3, User.Position.Z);
                ToHome.EnterWorld(PositionToPortal);


                yield break;
            }
        }
    }
}
