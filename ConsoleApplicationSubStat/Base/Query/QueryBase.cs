using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.IO;
using SvnObjects.Objects;
using System.Data.Linq;
using ConsoleApplicationSubStat.Base.Classes;
using System.Transactions;
using System.Diagnostics;

namespace ConsoleApplicationSubStat.Base.Query
{
    /// <summary>
    /// Facade for DB access and management
    /// </summary>
    public static class QueryBase
    {
        /// <summary>
        /// The name of database file
        /// </summary>
        private static readonly string dbName = "svn.db";

        //These collections are for storing added ids
        private static List<long?> addedrevisions = new List<long?>();
        private static Dictionary<string, long?> addedfiles = new Dictionary<string, long?>();
        private static Dictionary<string, long?> addedusers = new Dictionary<string, long?>();

        //first time program executes it gets max id from Files table and store it here
        // after program use this variable ++last_file_id to set every new row in the table
        private static long last_file_id = -1;

        //first time program executes it gets max id from Users table and store it here
        // after program use this variable ++last_user_id to set every new row in the table
        private static long last_user_id = -1;

        //first time program executes it gets max id from ChangedLinesCount table and store it here
        // after program use this variable ++last_user_id to set every new row in the table
        private static long last_changedlinescount_id = -1;

        /// <summary>
        /// Returns connection string to SQLite
        /// </summary>
        /// <returns></returns>
        public static string GetConnectionString()
        {
            SQLiteConnectionStringBuilder connStringBuilder = new SQLiteConnectionStringBuilder();
            connStringBuilder.DataSource = GetPathToBase();
            return connStringBuilder.ConnectionString;
        }


        /// <summary>
        /// Returns complete path to SQLite DB
        /// </summary>
        /// <returns></returns>
        public static string GetPathToBase()
        {
            string myBinPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return Path.Combine(myBinPath, dbName);
        }


        /// <summary>
        /// Constructs DataContext
        /// </summary>
        /// <returns></returns>
        private static DataContext ConstructContext()
        {
            // construct SQlite connection object
            var sqliteconneciton = new SQLiteConnection(GetConnectionString());
            DataContext context = new DataContext(sqliteconneciton);

#if DEBUG
            context.Log = Console.Out;
#endif

            return context;
        }

        public static long GetLastSavedRevisonNumber(DataContext context)
        {
            //Get revisions table
            var revisionstable = context.GetTable<RevisionDB>();
            var revision = revisionstable.Max<RevisionDB>(r => r.Revision);
            if (revision.HasValue)
                return revision.Value;

            return -1;

        }


        /// <summary>
        /// Adds the list of revisions to base 
        /// </summary>
        /// <param name="revisons"></param>
        public static void AddRevisionsToDB(IEnumerable<RepositoryInfo> revisons)
        {
            DataContext context = null;
            try
            {
               

                using (context = ConstructContext())
                {

                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    context.Connection.Open();
                    context.Transaction = context.Connection.BeginTransaction();

                    //get highest revision number from DB
                    long lastSavedRevision = GetLastSavedRevisonNumber(context);

                    //get from revision collection of all repositories with the index higher then saved one
                    //if we save from stratch, index is -1, so any revision present in collection will be higher
                    var revision_higher_then_saved = revisons.Where<RepositoryInfo>(re => re.Revision > lastSavedRevision);

                    //get resulting collection count
                    int revisionsCount = revision_higher_then_saved.Count<RepositoryInfo>();

                    Communicator.Communicator.NotifyRevisionsCountToInsert(revisionsCount);
                    foreach (var rev in revision_higher_then_saved)
                    {
                        AddRevisionToDB(context, rev);
                        context.SubmitChanges();

                        Communicator.Communicator.NotifyRevisionInesrtedInDBAndAvailableCount(rev, --revisionsCount);
                    }

                    context.Transaction.Commit();
                    context.Connection.Close();

                    stopWatch.Stop();
                    double ms = (stopWatch.ElapsedTicks * 1000.0) / Stopwatch.Frequency;
                    Console.WriteLine(string.Concat(ms.ToString(), " ms"));
                    Console.ReadLine();
                }

            }
            catch (TransactionAbortedException ex)
            {
                if (context != null && context.Transaction != null)
                    context.Transaction.Rollback();

                if (context.Connection != null)
                    context.Connection.Close();


                Console.WriteLine("TransactionAbortedException Message: {0}", ex.Message);
                Console.ReadLine();
            }
            catch (ApplicationException ex)
            {
                if (context != null && context.Transaction != null)
                    context.Transaction.Rollback();

                if (context.Connection != null)
                    context.Connection.Close();

                Console.WriteLine("ApplicationException Message: {0}", ex.Message);
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                if (context != null && context.Transaction != null)
                    context.Transaction.Rollback();

                if (context.Connection != null)
                    context.Connection.Close();

                Console.WriteLine("Exception Message: {0}", ex.Message);
                Console.ReadLine();
            }
            finally
            {
                addedrevisions.Clear();
                addedfiles.Clear();
                addedfiles.Clear();

            }

        }





        /// <summary>
        /// Adds speciied revision to base
        /// </summary>
        /// <param name="context"></param>
        /// <param name="repoInfo"></param>
        private static void AddRevisionToDB(DataContext context, RepositoryInfo repoInfo)
        {
            UserDB userDB = new UserDB(repoInfo);

            //check if user exists

            long iUserID = AddUser(context, repoInfo);

            AddRevision(context, repoInfo, iUserID);

            long fileID = AddFile(context, repoInfo);

            AddRevisionFile(context, repoInfo, fileID);

            AddChangedLinesCountOnFile(context, repoInfo, fileID);
        }


        static void AddChangedLinesCountOnFile(DataContext context, RepositoryInfo repoInfo, long fileid)
        {
            string replace = repoInfo.Item.Replace("/", @"\");

            DirectoryInfo dirinfo = Directory.GetParent(ProgramConfiguration.REPOSITORY_FOLDER.FolderPath);
            string repository_upper_directory = dirinfo.FullName;
            string local_path_to_item = repository_upper_directory + replace;
            int changedlinescount = Program.subFunc.GetChangedLinesCount(local_path_to_item, repoInfo.Item, repoInfo.Revision);
            if (changedlinescount > 0)
            {
                //Get revisions table
                var changedlinescounttable = context.GetTable<ChangedLinesCountDB>();
                ChangedLinesCountDB clc = new ChangedLinesCountDB();
                clc.Revision = repoInfo.Revision;
                clc.FileID = fileid;
                clc.LinesCount = changedlinescount;

                //get maximum ID available 
                if (last_changedlinescount_id < 0)
                {
                    long? ids = changedlinescounttable.Max<ChangedLinesCountDB>(u => u.ID);
                    last_changedlinescount_id = (ids.HasValue) ? ids.Value  : 0;
                }
                

                clc.ID = ++last_changedlinescount_id;
                changedlinescounttable.InsertOnSubmit(clc);

            }
        }

        /// <summary>
        /// Addds revision file to base
        /// </summary>
        /// <param name="context"></param>
        /// <param name="repoinfo"></param>
        /// <param name="fileID"></param>
        private static void AddRevisionFile(DataContext context, RepositoryInfo repoinfo, long fileID)
        {
            //Get revisions table
            var revisionfilestable = context.GetTable<RevisionFilesDB>();

            RevisionFilesDB revfileDB = new RevisionFilesDB(repoinfo);
            revfileDB.FileID = fileID;

            //get maximum ID available                 
            long? ids = revisionfilestable.Max<RevisionFilesDB>(rfd => rfd.ID);
            revfileDB.ID = (ids.HasValue) ? ids.Value + 1 : 1;
            revfileDB.Action  = CharFromState(repoinfo.ItemState);
           
            revisionfilestable.InsertOnSubmit(revfileDB);


        }


        static char CharFromState(RepositoryInfo.RepositoryItemState action)
        {
            if (action == RepositoryInfo.RepositoryItemState.Add)
                return 'A';
            else if (action == RepositoryInfo.RepositoryItemState.Conflict)
                return 'C';
            else if (action == RepositoryInfo.RepositoryItemState.Deleted)
                return 'D';
            else if (action == RepositoryInfo.RepositoryItemState.ExternalDefinition)
                return 'E';
            else if (action == RepositoryInfo.RepositoryItemState.Ignored)
                return 'I';
            else if (action == RepositoryInfo.RepositoryItemState.Merged)
                return '|';
            else if (action == RepositoryInfo.RepositoryItemState.Missing)
                return 'X';
            else if (action == RepositoryInfo.RepositoryItemState.Modified)
                return 'M';
            else if (action == RepositoryInfo.RepositoryItemState.NeedToBeUpdatedFromRepo)
                return 'R';
            else if (action == RepositoryInfo.RepositoryItemState.Normal)
                return 'N';
            else if (action == RepositoryInfo.RepositoryItemState.NotVersioned)
                return 'V';
            else if (action == RepositoryInfo.RepositoryItemState.Replaced)
                return '&';
            else if (action == RepositoryInfo.RepositoryItemState.VersionedWithDifferentKindOfObject)
                return 'O';

            return '-';
        }


        /// <summary>
        /// Adds file to the base if doesn't exist
        /// </summary>
        /// <param name="context"></param>
        /// <param name="repoInfo"></param>
        /// <returns>Returns an ID of specified file</returns>
        private static long AddFile(DataContext context, RepositoryInfo repoInfo)
        {
            //Get files table
            var filestable = context.GetTable<FileDB>();
            long? fid;

            //check first in memory collection 
            if (addedfiles.TryGetValue(repoInfo.Item, out fid))
                return fid.Value;

            //check if revision is already registered
            fid = (from file in filestable where file.File == repoInfo.Item select file.FileID).SingleOrDefault<long?>();

            if (!fid.HasValue)
            {
                FileDB fileDB = new FileDB(repoInfo);
                fileDB.File = repoInfo.Item;

                //get maximum ID available and store it global variable                
                if (last_file_id < 0)
                {
                    long? ids = filestable.Max<FileDB>(fdb => fdb.FileID);
                    last_file_id = (ids.HasValue) ? ids.Value : 0;
                }

                //increment variable and assign it to field
                fileDB.FileID = ++last_file_id;

                filestable.InsertOnSubmit(fileDB);


                fid = fileDB.FileID;

                addedfiles.Add(repoInfo.Item, fid);
            }


            return fid.Value;

        }

        /// <summary>
        /// Adds revison db object to base
        /// </summary>
        /// <param name="rev"></param>
        private static void AddRevision(DataContext context, RepositoryInfo repoinfo, long userid)
        {
            //Get revisions table
            var revisionstable = context.GetTable<RevisionDB>();

            //check first in memeory collection
            if (addedrevisions.Contains(repoinfo.Revision))
                return;

            //check if revision is already registered
            long? rev = (from revision in revisionstable where revision.Revision.Value == repoinfo.Revision select revision.Revision).SingleOrDefault<long?>();

            //revision doesn't exist in base, so save it in
            if (!rev.HasValue)
            {
                RevisionDB revDB = new RevisionDB(repoinfo);
                revDB.UserID = userid;
                revisionstable.InsertOnSubmit(revDB);

                addedrevisions.Add(repoinfo.Revision);
            }


        }


        /// <summary>
        /// Adds user specified in RepositoryInfo to the base, if it doesn't exist
        /// </summary>
        /// <param name="context"></param>
        /// <param name="repoInfo"></param>
        /// <returns>Returns an ID of specified user</returns>
        private static Int64 AddUser(DataContext context, RepositoryInfo repoInfo)
        {
            //construct UserDB object 
            UserDB udb = new UserDB(repoInfo);

            //Get users table
            var userstable = context.GetTable<UserDB>();
            long? userid;

            //first check in memory
            if (addedusers.TryGetValue(repoInfo.Account, out userid))
                return userid.Value;

            userid = (from u in userstable
                      where u.UserName == repoInfo.Account
                      select u.ID).SingleOrDefault<long?>();

            if (!userid.HasValue)
            {
                //get maximum ID available 
                if (last_user_id < 0)
                {
                    long? ids = userstable.Max<UserDB>(u => u.ID);
                    last_user_id = (ids.HasValue) ? ids.Value : 0;
                }

                udb.ID = ++last_user_id;
                userstable.InsertOnSubmit(udb);

                userid = udb.ID.Value;

                addedusers.Add(repoInfo.Account, udb.ID);

            }

            return userid.Value;

        }


    }
}
