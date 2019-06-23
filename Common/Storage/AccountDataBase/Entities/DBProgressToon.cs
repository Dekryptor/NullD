using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Data;
using NullD.Core.LogNet.Toons;

namespace NullD.Common.Storage.AccountDataBase.Entities
{
    public class DBProgressToon : Entity
    {
        public new virtual ulong Id { get; protected set; }
        public virtual DBGameAccount DBGameAccount { get; set; }
        public virtual DBToon DBToon { get; set; }
        public virtual int MaximumQuest { get; set; }
        public virtual int MaximumAct { get; set; }
        public virtual int ActiveQuest { get; set; }  
        public virtual int StepOfQuest { get; set; }
        public virtual int StepIDofQuest { get; set; }
        public virtual int Side_ActiveQuest { get; set; }
        public virtual int Side_StepOfQuest { get; set; }
        public virtual int Side_StepIDofQuest { get; set; }
        public virtual int ActiveAct { get; set; }

        public virtual string LoreCollected { get; set; }
        public virtual string AccessedQuestsNormal { get; set; }
        public virtual string AccessedQuestsNightmare { get; set; }
        public virtual string AccessedQuestsHell { get; set; }
        public virtual string AccessedQuestsInferno { get; set; }

        public virtual string TutorialsCollected { get; set; }

        public virtual int StatusOfWings { get; set; }

        public virtual int LastWorld { get; set; }
        public virtual int LastCheckPoint { get; set; }

        public virtual uint WaypointStatus { get; set; }
        public virtual uint WaypointStatus2 { get; set; }
        public virtual uint WaypointStatus3 { get; set; }
        public virtual uint WaypointStatus4 { get; set; }
        
    }
}
