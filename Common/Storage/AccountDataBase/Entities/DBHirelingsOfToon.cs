using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Data;
using NullD.Core.LogNet.Toons;

namespace NullD.Common.Storage.AccountDataBase.Entities
{
    public class DBHirelingsOfToon : Entity
    {
        public new virtual ulong Id { get; protected set; }
        public virtual DBGameAccount DBGameAccount { get; set; }
        public virtual DBToon DBToon { get; set; }
        public virtual int Level_Enchantress { get; set; }
        public virtual int Experience_Enchantress { get; set; }
        public virtual int Level_Scoundrel { get; set; }
        public virtual int Experience_Scoundrel { get; set; }
        public virtual int Level_Templar { get; set; }
        public virtual int Experience_Templar { get; set; }

        public virtual int Enchantress_Skill1 { get; set; }
        public virtual int Enchantress_Skill2 { get; set; }
        public virtual int Enchantress_Skill3 { get; set; }
        public virtual int Enchantress_Skill4 { get; set; }

        public virtual int Scoundrel_Skill1 { get; set; }
        public virtual int Scoundrel_Skill2 { get; set; }
        public virtual int Scoundrel_Skill3 { get; set; }
        public virtual int Scoundrel_Skill4 { get; set; }

        public virtual int Templar_Skill1 { get; set; }
        public virtual int Templar_Skill2 { get; set; }
        public virtual int Templar_Skill3 { get; set; }
        public virtual int Templar_Skill4 { get; set; }

    }
}
