using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using NullD.Common.Storage.AccountDataBase.Entities;

namespace NullD.Common.Storage.AccountDataBase.Mapper
{
    public class DBPortalOfToonMapper : ClassMap<DBPortalOfToon>
    {
        public DBPortalOfToonMapper()
        {
            Id(e => e.Id).GeneratedBy.Native();
            References(e => e.DBGameAccount).Nullable();
            References(e => e.DBToon).Nullable();
            Map(e => e.X);
            Map(e => e.Y);
            Map(e => e.Z);
            Map(e => e.WorldDest);

        }
    }
}