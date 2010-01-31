/* LogManager.cs --------------------------------
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
using System.Windows;

namespace SubversionStatistics.Util
{
    /// <summary>
    /// Manages loggin of the application with predefined format
    /// </summary>
    public static class LogManager
    {
        #region fields 
        static readonly string LogFileCommonName = "rb";
        static readonly string LogFileExtension = "log";
        static readonly string LogDirectoryName = "AppLog";
        static string logFileFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        #endregion

        #region message state enum
        public enum LogMessageStatus { Message, Error };
        #endregion

        #region log methods 
        /// <summary>
        /// Log the string in the log file with the predefined data format
        /// </summary>
        /// <param name="logString">The string to log</param>
        /// <param name="status">The status of the message to insert</param>
        public static void LogThis(string logString, LogMessageStatus status)
        {
            try
            {
                string todayLogFileName = GetTodayLogFileName();
                if (string.IsNullOrEmpty(todayLogFileName))
                    throw new ArgumentException("Can not generate today's log file name");

                
                /*Check log directory existance, if it's not present create it */
                string DirCompletePath = logFileFolder + System.IO.Path.DirectorySeparatorChar +
                    LogDirectoryName;
                if (!System.IO.Directory.Exists(DirCompletePath))
                {
                    System.IO.Directory.CreateDirectory(DirCompletePath);
                }

                DateTime dtNow = DateTime.Now;
                string statusString = "Message";
                if(status == LogMessageStatus.Error) 
                    statusString = "Error";
                
                string logStringData = "Data: " + dtNow.ToLongDateString() + " : " + dtNow.ToLongTimeString()  + " " + System.Environment.NewLine + 
                    "Status: " + statusString + " " + System.Environment.NewLine + 
                    "Message: " + logString + System.Environment.NewLine;


                /*Check log today's file existance, if it's not present create it */
                string todaysLogFileCompletePath = DirCompletePath + System.IO.Path.DirectorySeparatorChar  + 
                    todayLogFileName + "." + LogFileExtension;                

               
               System.IO.File.AppendAllText(todaysLogFileCompletePath, logStringData, Encoding.UTF8);
                   
               

              

            }
            catch(Exception ex)
            {
                ErrorManager.ShowExceptionError(ex, false);
            }
        }


        /// <summary>
        ///Returns special formatted string of the current date and time
        /// </summary>
        /// <returns></returns>
        static string GetTodayLogFileName()
        {
            DateTime dt = DateTime.Now;
            return LogFileCommonName + "-" + dt.Year + "-" + dt.Month + "-" + dt.Day;
        }

        #endregion
    }
}
