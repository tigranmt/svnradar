using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;
using SvnObjects.Objects;
using System.Data.Metadata.Edm;

namespace ConsoleApplicationSubStat.Base.Classes
{
    /// <summary>
    /// Class holds User's log information which will be stored in DB
    /// </summary>
    [Table(Name = "Users")]
    public sealed class UserDB
    {
        public UserDB() { }

        public UserDB(RepositoryInfo repoInfo)
        {
            UserName = repoInfo.Account;
        }

        [Column(Name = "ID", IsPrimaryKey=true, AutoSync = AutoSync.Never, CanBeNull = false)]
        public long? ID { get; set; }

        [Column(Name = "UserName", DbType="nvarchar(50)")]
        public string UserName { get; set ; }
    }
}
