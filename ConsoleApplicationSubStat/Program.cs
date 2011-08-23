using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using SvnObjects.Objects;
using ConsoleApplicationSubStat.Base.Query;

namespace ConsoleApplicationSubStat
{
    class Program
    {
        static readonly string sDBFileName = "stat.db";
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
            string sSubversionRepositoryPath = args[1];

            ///Sample for query Subverion repository
            SvnObjects.SvnFunctions.SubversionFunctions subFunc = new SvnObjects.SvnFunctions.SubversionFunctions(sSubversionExePath);
            FolderRepoInfo info = subFunc.GetFolderRepoInfo(sSubversionRepositoryPath, -1);
            List<RepositoryInfo> lRepository = subFunc.GetRepositoryLogImmediate(info, -1);
            
            QueryBase.AddRevisionsToDB(lRepository);
         
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
