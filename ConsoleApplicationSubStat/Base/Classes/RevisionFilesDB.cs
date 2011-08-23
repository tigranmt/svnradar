using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;
using SvnObjects.Objects;

namespace ConsoleApplicationSubStat.Base.Classes
{
    [Table(Name = "RevisionFiles")]
    public sealed class RevisionFilesDB
    {

        public RevisionFilesDB() { }

        public RevisionFilesDB(RepositoryInfo repoinfo)
        {
            this.Revision = repoinfo.Revision;
        }

        [Column(Name = "ID", IsPrimaryKey = true)]
        public long? ID { get; set; }

        [Column(Name = "FileID")]
        public long FileID { get; set; }

        [Column(Name = "Revision")]
        public long Revision { get; set; }
    }
}
