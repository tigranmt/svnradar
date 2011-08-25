using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;

namespace ConsoleApplicationSubStat.Base.Classes
{

    /// <summary>
    /// Class holds File's log information which will be stored in DB
    /// </summary>
    [Table(Name = "ChangedLinesCount")]
    public sealed class ChangedLinesCountDB
    {
         public ChangedLinesCountDB() { }      

        [Column(Name = "ID",  UpdateCheck = UpdateCheck.Never, IsPrimaryKey=true, AutoSync = AutoSync.Never, CanBeNull = false)]
        public long? ID { get; set; }

        [Column(Name = "FileID",UpdateCheck = UpdateCheck.Never)]
        public long FileID { get; set ; }

        [Column(Name = "Revision", UpdateCheck = UpdateCheck.Never)]
        public long Revision { get; set; }

        [Column(Name = "LinesCount", UpdateCheck = UpdateCheck.Never)]
        public int LinesCount { get; set; }
    }
}
