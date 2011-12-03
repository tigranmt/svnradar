/* UserDB.cs --------------------------------
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

        [Column(Name = "ID",  UpdateCheck = UpdateCheck.Never, IsPrimaryKey=true, AutoSync = AutoSync.Never, CanBeNull = false)]
        public long? ID { get; set; }

        [Column(Name = "UserName",UpdateCheck = UpdateCheck.Never, DbType="nvarchar(50)")]
        public string UserName { get; set ; }
    }
}
