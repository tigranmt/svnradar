﻿/* BugReportData.cs --------------------------------
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
using System.ComponentModel;

namespace SvnRadar.Util
{
    /// <summary>
    /// Holds all necessary information about user or system defined bug/comment
    /// </summary>
    internal sealed class BugReportData : INotifyPropertyChanged
    {
        #region fields 

        public enum MessageMotivation { Error, UserComment };

        string reporterName = string.Empty;
        string userComment = string.Empty;
        MessageMotivation motivation = MessageMotivation.Error;

        /// <summary>
        /// Maximum comment length in chars
        /// </summary>
        public static readonly int MAX_AVAILABLE_COMMENT_LENGTH = 500;
        #endregion 


        #region properties

        /// <summary>
        /// Reporter name
        /// </summary>
        public string ReporterName
        {
            get { return reporterName; }
            set 
            {                 
               
                /*Raise an exception on empty value, so the Binding exception validator will catch it and higlight an lement of interest on the
                 user screen in order to notify to him about mandatory field that need to be compiled */
                if (string.IsNullOrEmpty(value))
                {
                    string appExcetionString = AppResourceManager.FindResource("MSG_REPORTERNAMEMANDATORY") as string;
                    throw new ApplicationException(appExcetionString);
                }

                reporterName = value;
            
            }
        }

        /// <summary>
        /// User comment
        /// </summary>
        public string Comment
        {
            get { return userComment; }
            set 
            {
               
                /*Raise an exception on empty value, so the Binding exception validator will catch it and higlight an lement of interest on the
                user screen in order to notify to him about mandatory field that need to be compiled */
                if (string.IsNullOrEmpty(value))
                {
                    string appExcetionString = AppResourceManager.FindResource("MSG_MESSAGEMANDATORY") as string;
                    throw new ApplicationException(appExcetionString);
                }

                userComment = value;

                /*Raise RemainingCharsCount property change as the content already was changed by itself*/
                OnPropertyChanged("RemainingCharsCount");
            
            }
        }


        /// <summary>
        /// Attached file's complete path
        /// </summary>
        public string AttachedFile { get; set; }

        /// <summary>
        /// Gets teh count of chars that can be yet inserted in the user comment string. This depends on MAX_AVAILABLE_COMMENT_LENGTH consts definition.
        /// </summary>
        public int RemainingCharsCount
        {
            get
            {
                if (string.IsNullOrEmpty(userComment))
                    return MAX_AVAILABLE_COMMENT_LENGTH;
                else
                {
                    return (MAX_AVAILABLE_COMMENT_LENGTH - userComment.Length);
                }
            }
        }


        public int MaxCommentLength
        {
            get { return MAX_AVAILABLE_COMMENT_LENGTH; }
        }


        public MessageMotivation Motivation
        {
            get { return motivation; }
            set { 


                motivation = value;

                OnPropertyChanged("Motivation");
            
            }
        }

        #endregion


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// OnPropertyChanged for raising INotifyPropertyChanged inetrface notification
        /// </summary>
        /// <param name="name"></param>
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

    }
}
