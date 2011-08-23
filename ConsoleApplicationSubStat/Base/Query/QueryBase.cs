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
        private static Dictionary<string,long?> addedusers = new Dictionary<string,long?>();


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

                    context.ObjectTrackingEnabled = false;

                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    context.Connection.Open();
                    context.Transaction = context.Connection.BeginTransaction();

                    int revisionsCount = revisons.Count<RepositoryInfo>();
                    Communicator.Communicator.NotifyRevisionsCountToInsert(revisionsCount);
                    foreach (var rev in revisons)
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

            revisionfilestable.InsertOnSubmit(revfileDB);
           
           
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

                //get maximum ID available                 
                long? ids = filestable.Max<FileDB>(fdb => fdb.FileID);
                fileDB.FileID = (ids.HasValue) ? ids.Value + 1 : 1;

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
            long?userid;

            //first check in memory
            if (addedusers.TryGetValue(repoInfo.Account, out userid))
                return userid.Value;

            userid = (from u in userstable
                          where u.UserName == repoInfo.Account
                          select u.ID).SingleOrDefault<long?>();

            if (!userid.HasValue)
            {
                //get maximum ID available 
                long? ids = userstable.Max<UserDB>(u => u.ID);
                udb.ID = (ids.HasValue) ? ids.Value + 1 : 1;
                userid = udb.ID.Value;

                userstable.InsertOnSubmit(udb);     

                addedusers.Add(repoInfo.Account, udb.ID);               
            }

            return userid.Value;

        }


    }
}
