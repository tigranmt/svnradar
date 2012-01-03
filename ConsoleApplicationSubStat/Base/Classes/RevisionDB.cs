/* RevisionDB.cs --------------------------------
 * 
 * * Copyright (c) 2009 Tigran Martirosyan 
 * * Contact and Information: tigranmt@gmail.com 
 * * This application is free software; you can redistribute it and/or 
 * * Modify it under the terms of the GPL license
 * * THE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND, 
 * * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
 * * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
 * * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
 * * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR 
 * * OTHER DEALINGS IN THE SOFTWARE. 
 * * THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE 
 *  */
// ---------------------------------

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
            if(DateTime.TryParse(repoInfo.Date, System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None, out dateToSave))
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
