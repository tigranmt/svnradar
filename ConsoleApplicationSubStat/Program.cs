using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using SvnObjects.Objects;
using ConsoleApplicationSubStat.Base.Query;

namespace ConsoleApplicationSubStat
{

    public static class ProgramConfiguration
    {       
        public static FolderRepoInfo REPOSITORY_FOLDER = null;
    }

    class Program
    {
        static readonly string sDBFileName = "stat.db";
        internal static SvnObjects.SvnFunctions.SubversionFunctions subFunc = null;
      

        static void Main(string[] args)
        {

            if (args == null || args.Length < 2)
            {
                WriteError("Not enough arguments for running stat. Use -h to gte help on possible arguments.");
                Console.ReadLine();
                return;
            }


            if (!System.IO.File.Exists(args[0]))
            {
                WriteError("Not valid subversion executable path : " + args[0]);
                Console.ReadLine();
                return;
            }

            if (!System.IO.Directory.Exists(args[1]))
            {
                WriteError("Not valid subversion repository directory path : " + args[0]);
                Console.ReadLine();
                return;
            }

            string sSubversionExePath = args[0];
            string repository_local_path = args[1];


          


            //save current directory for system 
           // string currentDirectory = System.IO.Directory.GetCurrentDirectory();

            //set current directory for system to subversion repository local path 
           // System.IO.Directory.SetCurrentDirectory(ProgramConfiguration.REPOSITORY_LOCAL_PATH);

            ///Sample for query Subverion repository
            subFunc = new SvnObjects.SvnFunctions.SubversionFunctions(sSubversionExePath);

             //first update repository to latest version 
            bool updatesucceed = subFunc.UpdateRepository(repository_local_path);
            if (!updatesucceed)
            {
                WriteError("Failed update repository. Can not generate statistics on this repository. Please fix the problem and run the program again.");
                Console.ReadLine();
                return;
            }
            ProgramConfiguration.REPOSITORY_FOLDER = subFunc.GetFolderRepoInfo(repository_local_path, -1);
            List<RepositoryInfo> lRepository = subFunc.GetRepositoryLogImmediate(ProgramConfiguration.REPOSITORY_FOLDER, -1);
            
            //reverse collection to begin insert from lowest revision available
            lRepository.Reverse();

            QueryBase.AddRevisionsToDB(lRepository);

            //restore previously saved current directory for system
          //  System.IO.Directory.SetCurrentDirectory(currentDirectory);
         
        }



        private static void WriteError(string sErroString)
        {
            WriteString(sErroString, ConsoleColor.Red);
        }


        private static void WriteWarning(string sWarning)
        {
            WriteString(sWarning, ConsoleColor.Yellow);
        }


        private static void WriteMessage(string sMessage)
        {
            WriteString(sMessage, ConsoleColor.White);
        }


        private static void WriteString(string sString, ConsoleColor color) 
        {
            ConsoleColor curColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(sString);
            Console.ForegroundColor = curColor;
        }
    }
}
