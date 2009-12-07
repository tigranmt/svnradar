/* ErrorManager.cs --------------------------------
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
using System.Diagnostics;

namespace SvnRadar.Util
{
    /// <summary>
    /// Manages general visulaistion and data management of the appication raised error in roder to notify or log the error data
    /// </summary>
    internal static class ErrorManager
    {

        #region show error methods
        /// <summary>
        /// Shows general information about the specifed process reposrted error
        /// </summary>
        /// <param name="process">Process object that signaled abouit an error</param>
        public static void ShowProcessError(Process process)
        {
            if (process == null)
                throw new ArgumentException("The process object is Null");

            if (process.StartInfo == null)
                throw new ArgumentException("The process's StartInfo object is Null");

            if (process.StartInfo.RedirectStandardError == false)
                throw new ArgumentException("The process's StartInfo object's RedirectStandardError property is False." +
                                    " It have to be set to True in order to read the standart error stream.");


            string stdErrorMessage = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(stdErrorMessage))
            {
                System.Windows.MessageBox.Show(stdErrorMessage, "Error", System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);

                LogMessage(stdErrorMessage, true);
            }

        }



        /// <summary>
        /// Shows common error dialog filled with secified string
        /// </summary>
        /// <param name="strToShow">String to show on erro dialog</param>     
        /// <param name="bLog">If true, the string will be logged too</param>
        public static void ShowExceptionError(Exception ex, bool bLog)
        {
            if (ex == null)
                return;

            string strMessage = "Exception message: " + ex.Message +
                System.Environment.NewLine +
                "Exception stack trace: " + ex.StackTrace.ToString() +
                System.Environment.NewLine;

            System.Windows.MessageBox.Show(strMessage, "Error", System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);


            if (bLog)
                LogMessage(strMessage, true);

        }




        /// <summary>
        /// Shows common error dialog filled with secified string
        /// </summary>
        /// <param name="strToShow">String to show on erro dialog</param>
        /// <param name="bLog">If true, the string will be logged too</param>
        public static void ShowCommonError(string strToShow, bool bLog)
        {
            System.Windows.MessageBox.Show(strToShow, "Error", System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);

            if (bLog)
                LogMessage(strToShow, true);

        }




        /// <summary>
        /// Shows common error dialog filled with secified string and yesNo buttons choise
        /// </summary>
        /// <param name="strToShow">String to show on erro dialog</param>
        /// <param name="bLog">If true, the string will be logged too</param>
        public static System.Windows.MessageBoxResult ShowCommonErrorYesNo(string strToShow, bool bLog)
        {
            System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show(strToShow, "Error", System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Error);

            if (bLog)
                LogMessage(strToShow, true);


            return result;

        }

        #endregion

        #region logging methods
        /// <summary>
        /// Logs the pecified string in the main log  file of application.
        /// </summary>
        /// <param name="logTheError">String to log</param>
        /// <param name="isError">Detrmines if the message is erro message, so the correposnding log message string will have related parameter</param>
        public static void LogMessage(string logString, bool isError)
        {
            if (string.IsNullOrEmpty(logString))
                return;

            if (isError)
                LogManager.LogThis(logString, LogManager.LogMessageStatus.Error);
            else
                LogManager.LogThis(logString, LogManager.LogMessageStatus.Message);

        }

        /// <summary>
        /// Logs the pecified exception object in the main log  file of application.
        /// </summary>
        /// <param name="ex">Exception object to log</param>
        public static void LogException(Exception ex)
        {
            if (ex == null)
                return;

            string strExceptionMessage = "Exception message: " + ex.Message +
               System.Environment.NewLine +
               "Exception stack trace: " + ex.StackTrace.ToString() +
               System.Environment.NewLine;

            LogMessage(strExceptionMessage, true);

        }

        #endregion

    }
}
