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

        [Column(Name = "ID", UpdateCheck = UpdateCheck.Never, IsPrimaryKey = true)]
        public long? ID { get; set; }

        [Column(Name = "FileID", UpdateCheck = UpdateCheck.Never)]
        public long FileID { get; set; }

        [Column(Name = "Revision", UpdateCheck = UpdateCheck.Never)]
        public long Revision { get; set; }


        [Column(Name = "Action", UpdateCheck = UpdateCheck.Never)]
        public char Action { get; set; }
    }
}
