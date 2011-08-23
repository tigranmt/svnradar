using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;
using SvnObjects.Objects;

namespace ConsoleApplicationSubStat.Base.Classes
{
    /// <summary>
    /// Class holds Revision's log information which will be stored in DB
    /// </summary>
    [Table(Name = "Revisions")]
    public sealed class RevisionDB
    {

        public RevisionDB() { }

        /// <summary>
        /// Constructs revision map DB object from RepositoryInfo type
        /// </summary>
        /// <param name="repoInfo"></param>
        public  RevisionDB(RepositoryInfo repoInfo)
        {
            Revision = repoInfo.Revision;
            UserID = 0;
            Comment = repoInfo.UserComment;

            DateTime dateToSave;
            if(DateTime.TryParse(repoInfo.Date, out dateToSave))
                Date = dateToSave;
        }

     

        [Column(Name = "Revision", UpdateCheck = UpdateCheck.Never, IsPrimaryKey = true)]
        public long? Revision { get; set; }

        [Column(Name = "UserID", UpdateCheck = UpdateCheck.Never)]
        public Int64 UserID { get; set; }

        [Column(Name = "Comment", UpdateCheck = UpdateCheck.Never)]
        public string Comment { get; set; }

        [Column(Name = "Date", UpdateCheck = UpdateCheck.Never)]
        public DateTime Date { get; set; }
    }
}
