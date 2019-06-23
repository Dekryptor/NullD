using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using NullD.Common.Storage.AccountDataBase.Entities;

namespace NullD.Common.Storage.AccountDataBase.Mapper
{
    public class DBProgressToonMapper : ClassMap<DBProgressToon>
    {
        public DBProgressToonMapper()
        {
            Id(e => e.Id).GeneratedBy.Native();
            References(e => e.DBGameAccount).Nullable();
            References(e => e.DBToon).Nullable();
            Map(e => e.MaximumQuest);
            Map(e => e.MaximumAct);
            Map(e => e.ActiveQuest);
            Map(e => e.StepOfQuest);
            Map(e => e.StepIDofQuest);
            Map(e => e.Side_ActiveQuest);
            Map(e => e.Side_StepOfQuest);
            Map(e => e.Side_StepIDofQuest);
            Map(e => e.ActiveAct);
            Map(e => e.WaypointStatus);
            Map(e => e.WaypointStatus2);
            Map(e => e.WaypointStatus3);
            Map(e => e.WaypointStatus4);

            Map(e => e.StatusOfWings);
            Map(e => e.LoreCollected);

            Map(e => e.AccessedQuestsNormal);
            Map(e => e.AccessedQuestsNightmare);
            Map(e => e.AccessedQuestsHell);
            Map(e => e.AccessedQuestsInferno);

            Map(e => e.TutorialsCollected);

            Map(e => e.LastWorld);
            Map(e => e.LastCheckPoint);
        }
    }
}