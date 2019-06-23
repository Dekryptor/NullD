using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using NullD.Common.Storage.AccountDataBase.Entities;

namespace NullD.Common.Storage.AccountDataBase.Mapper
{
    public class DBHirelingsOfToonMapper : ClassMap<DBHirelingsOfToon>
    {
        public DBHirelingsOfToonMapper()
        {
            Id(e => e.Id).GeneratedBy.Native();
            References(e => e.DBGameAccount).Nullable();
            References(e => e.DBToon).Nullable();
            Map(e => e.Level_Enchantress);
            Map(e => e.Experience_Enchantress);
            Map(e => e.Level_Scoundrel);
            Map(e => e.Experience_Scoundrel);
            Map(e => e.Level_Templar);
            Map(e => e.Experience_Templar);

            Map(e => e.Enchantress_Skill1);
            Map(e => e.Enchantress_Skill2);
            Map(e => e.Enchantress_Skill3);
            Map(e => e.Enchantress_Skill4);

            Map(e => e.Scoundrel_Skill1);
            Map(e => e.Scoundrel_Skill2);
            Map(e => e.Scoundrel_Skill3);
            Map(e => e.Scoundrel_Skill4);

            Map(e => e.Templar_Skill1);
            Map(e => e.Templar_Skill2);
            Map(e => e.Templar_Skill3);
            Map(e => e.Templar_Skill4);

        }
    }
}