using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SvnObjects.Objects;

namespace ConsoleApplicationSubStat.Base.Communicator
{
    public static class Communicator
    {
        public static void NotifyRevisionInsertedInDB(RepositoryInfo revinfo)
        {
            NotifyHeader(string.Format("Insert r[{0}]", revinfo.Revision),true);
        }

        public static void NotifyRevisionsCountToInsert(int revcount)
        {
            NotifyMessage(string.Format("Insert count [{0}]", revcount), true);           
        }


        public static void NotifyRevisionInesrtedInDBAndAvailableCount(RepositoryInfo revinfo, int countToInsert)
        {
            NotifyHeader(string.Format("Insert r[{0}]", revinfo.Revision), false);
            NotifyHeader("  ", false);
            NotifyMessage(string.Format("Insert count [{0}]", countToInsert), false);
            NotifyHeader("", true);

        }

        private static void NotifyHeader(string header, bool newline)
        {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            if (newline)
                Console.WriteLine(header);
            else
                Console.Write(header);
            Console.ForegroundColor = currentColor;
        }

        private static void NotifyMessage(string message, bool newline)
        {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            if (newline)
                Console.WriteLine(message);
            else
                Console.Write(message);
            Console.ForegroundColor = currentColor;
        }

        private static void NotifyError(string error, bool newline)
        {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            if (newline)
                Console.WriteLine(error);
            else
                Console.Write(error);
            Console.ForegroundColor = currentColor;
        }
    }
}
