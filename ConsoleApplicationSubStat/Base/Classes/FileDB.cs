using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;
using SvnObjects.Objects;

namespace ConsoleApplicationSubStat.Base.Classes
{
    /// <summary>
    /// Class holds File's log information which will be stored in DB
    /// </summary>
    [Table(Name = "Files")]
    public sealed class FileDB
    {        
        public FileDB() { }

        public FileDB(RepositoryInfo repoinfo)
        {
            this.File = repoinfo.Item;
        }

        [Column(IsPrimaryKey = true, Name = "FileID", UpdateCheck = UpdateCheck.Never)]
        public long? FileID { get; set; }

        [Column(Name = "File", UpdateCheck = UpdateCheck.Never)]
        public string File { get; set; }
    }
}
