/* Communicator.cs --------------------------------
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
