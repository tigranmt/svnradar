/* TaskNotifier.cs --------------------------------
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
using Hardcodet.Wpf.TaskbarNotification;
using SvnRadar.Common.Controls;
using System.Threading;
using System.Windows.Threading;
using System.Windows;
using System.Collections.ObjectModel;
using SvnObjects;


namespace SvnRadar.Util
{
    /// <summary>
    /// Manages interaction and model of the Task notifier
    /// </summary>
    public class TaskNotifierManager
    {
        #region fields
        /// <summary>
        /// UI element that wraps NotificationIcon
        /// </summary>
        static TaskbarIcon taskBarIcon = null;

        /// <summary>
        /// Notification list content of wich have to be segnalized to the user
        /// </summary>
        internal static ObservableCollection<FolderRepoInfo> notificationList = new ObservableCollection<FolderRepoInfo>();

      
        ///// <summary>
        ///// Timer for flip the icon on sys tray
        ///// </summary>
        //static System.Timers.Timer fliptimer = new System.Timers.Timer();


    
        /// <summary>
        /// Index of the current FolderRepoInfo object being processed in the list
        /// </summary>
        static int currentNotificationIndex = 0;


        
        #endregion





        #region ctor
        static TaskNotifierManager()
        {
           
        }
        #endregion

     

      


        /// <summary>
        /// Fancvcy Balloon static object
        /// </summary>
        static FancyBalloon fancyBalloon = null;


        /// <summary>
        /// The balloon window that appears in system tray
        /// </summary>
        public static FancyBalloon Balloon
        {
            get
            {
                return fancyBalloon;
            }
        }


        /// <summary>
        /// Set ups TaskBarIcon object 
        /// </summary>
        /// <param name="taskBarIcon">TaskBarItem object to manage</param>
        public static void SetTaskBarIconObject(TaskbarIcon taskBarIconObject)
        {
            taskBarIcon = taskBarIconObject;
        }



        /// <summary>
        /// Shows first notification if exists
        /// </summary>
        public static void ShowFirstChangeIfThereIs()
        {
            if (notificationList == null || notificationList.Count == 0)
                return;

            ShowNotification(notificationList[0]);
            SignalChangesOnSysTray(true);
            
        }


        /// <summary>
        /// Set error icon on sys tray
        /// </summary>
        public static void SetErrorIcon()
        {
            taskBarIcon.SetErrorIcon();
        }

        /// <summary>
        /// Notifies to the ballon the repository that is up to date.
        /// </summary>
        /// <param name="upToDateRepositoryCompletePath">The repository working copy complete path</param>
        public static void UpToDateRepository(string upToDateRepositoryCompletePath)
        {
            if (string.IsNullOrEmpty(upToDateRepositoryCompletePath))
                return;

            /*need to update notiication list, by removing all that repositories that are up to date*/
            IEnumerable<FolderRepoInfo> repoInfoListToRemove = null; 
            lock (notificationList)
            {
                repoInfoListToRemove = notificationList.Where<FolderRepoInfo>((x) => x.FolderPath.Trim().Equals(upToDateRepositoryCompletePath.Trim(),StringComparison.InvariantCultureIgnoreCase));
                
                if (repoInfoListToRemove != null)
                {
                    foreach (FolderRepoInfo fri in repoInfoListToRemove)
                    {
                        int removalIndex = notificationList.IndexOf(fri);
                       
                        notificationList.Remove(fri);

                        if (notificationList.Count > 0)
                        {
                            if (removalIndex == currentNotificationIndex)
                            {
                                if (currentNotificationIndex > 0)
                                    --currentNotificationIndex;
                                else
                                {
                                    currentNotificationIndex = notificationList.Count - 1;
                                }

                            }
                        }

                        /*There couldn't be more then one repository */
                        break;

                    }
                }

               
            }


            //Could be possible during the closing of the main window
            if (Application.Current == null)
                return;

            /*Updates counter textbox*/
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                    BindToDataContextFromArray(currentNotificationIndex);
                    UpdateCounterTextBox();
            }));
         
        }

        #region flip timer
        /// <summary>
        /// Begins the flipping og the sys tray icon, if it's not already begun, in order 
        /// to attract user attention to the fact that there were found some repositories
        /// that got changes on the server.
        /// </summary>
        /// <param name="signal">Pass True to begin sys tray icon signal, False to stop. </param>
        public static void SignalChangesOnSysTray(bool signal)
        {
            if (taskBarIcon == null)
                throw new NullReferenceException("TaskBarIcon object is Null. Call SetTaskBarIconObject to setup the reference");

            if (signal)
            {

                /*Updates counter textbox*/
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if(ErrorManager.SilentNotificationList.Count ==0)
                        taskBarIcon.FlipIcon(true);
                   
                }));

                
            }
            else
            {


                /*Updates counter textbox*/
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (ErrorManager.SilentNotificationList.Count == 0)
                        taskBarIcon.FlipIcon(false);
                    notificationList.Clear();
                }));
               
               
            }



        }


        #endregion


        /// <summary>
        /// Add the chanes information to the notification list
        /// </summary>
        /// <param name="information">FolderRepoInfo object that contains the informatn about made changes </param>
        public static void AddToNitificationList(FolderRepoInfo information)
        {
            if (information == null)
                return;

            /*First remove from the collection, if already exists, after add*/
            FolderRepoInfo savedInfo = notificationList.FirstOrDefault<FolderRepoInfo>((x) => x.Url.Equals(information.Url, StringComparison.InvariantCultureIgnoreCase));

            if (savedInfo != null)
            {
                /*If upcoming information is fresher, update it in internal collection*/
                if (savedInfo.Revision < information.Revision)
                {
                    notificationList.Remove(savedInfo);
                    notificationList.Add(information);


                    /*Show the notification on system tray*/
                 //   ShowNotification(information);

                }


            }
            else
            {
                ///*Show the notification on system tray*/

                notificationList.Add(information);
               
                ///*Show the notification on system tray*/
              //  ShowNotification(information);
               
            }
        }


        /// <summary>
        /// Initialize balloon interanl object and subscribing to events
        /// </summary>
        public static void InitBalloon()
        {
            fancyBalloon = new FancyBalloon();

            /*Subscribing to events */
            fancyBalloon.RepositoryUpdateRequested += new FancyBalloon.RepositoryRoutedEventHandler(fancyBalloon_RepositoryUpdateRequested);
            fancyBalloon.RepositoryChangesViewRequested += new FancyBalloon.RepositoryRoutedEventHandler(fancyBalloon_RepositoryChangesViewRequested);
            fancyBalloon.NextChangeViewRequested += new RoutedEventHandler(fancyBalloon_NextChangeViewRequested);
            fancyBalloon.PrevChangeViewRequested += new RoutedEventHandler(fancyBalloon_PrevChangeViewRequested);
        }

        /// <summary>
        /// Notify popup window in system tray about specifed repository information
        /// </summary>
        /// <param name="information">Folder repository informatin to show on sys tray popup</param>       
        static void ShowNotification(FolderRepoInfo information)
        {
            if (taskBarIcon == null)
                throw new NullReferenceException("TaskBarIcon object is Null. Call SetTaskBarIconObject to setup the reference");



            if (fancyBalloon == null)
            {
                InitBalloon();
            }

            if (fancyBalloon.IsVisible)
            {
                //PushInfoOnWait(information);
                UpdateCounterTextBox();
                return;
            }

            /*Set data binding on fancy balloon*/
            fancyBalloon.DataContext = information;

            taskBarIcon.ShowCustomBalloon(fancyBalloon, System.Windows.Controls.Primitives.PopupAnimation.Slide, 5000);

            UpdateCounterTextBox();
        }


        /// <summary>
        /// Handles prev change button routed event 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void fancyBalloon_PrevChangeViewRequested(object sender, RoutedEventArgs e)
        {
            SetPrevInfoOnDataContext();
        }


        /// <summary>
        /// Sets the prev information in the array on balloon's DataContext, if we at the start of the array, the  jump 
        /// to the las element will be executed
        /// </summary>
        static void SetPrevInfoOnDataContext()
        {
            lock (notificationList)
            {
                if (notificationList.Count == 0)
                    return;

                /*decrement index*/
                --currentNotificationIndex;

                /*make a cycle*/
                if (currentNotificationIndex < 0)
                    currentNotificationIndex = notificationList.Count - 1;

                BindToDataContextFromArray(currentNotificationIndex);

            }

            UpdateCounterTextBox();
        }

        /// <summary>
        /// Handles next change button routed event 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void fancyBalloon_NextChangeViewRequested(object sender, RoutedEventArgs e)
        {
            SetNextInfoOnDataContext();
        }



        /// <summary>
        /// Sets the next information in the array on balloon's DataContext, if we at the end of the array, the  jump 
        /// to the first element will be executed
        /// </summary>
        static void SetNextInfoOnDataContext()
        {
            lock (notificationList)
            {
                if (notificationList.Count == 0)
                    return;

                /*decrement index*/
                ++currentNotificationIndex;

                /*make a cycle*/
                if (currentNotificationIndex == notificationList.Count)
                    currentNotificationIndex = 0;


                BindToDataContextFromArray(currentNotificationIndex);


            }


            UpdateCounterTextBox();
        }

        /// <summary>
        /// Binds the  FolderRepoInfo object, picked from the array by speciied index, to the balloon DataContext.
        /// </summary>
        /// <param name="index">Array index of an elemnt to be assigned to DataContext</param>
        static void BindToDataContextFromArray(int index)
        {
            if (notificationList == null || notificationList.Count == 0)
            {
               
                /*Set data binding on fancy balloon*/
                fancyBalloon.DataContext = null;
                taskBarIcon.CloseBalloon();
                taskBarIcon.FlipIcon(false);
                return;
            }

            /*If index is out of array bounds, assign 0*/
            if(currentNotificationIndex  >= notificationList.Count) 
            {
                currentNotificationIndex = 0;
            }

            /*Set data binding on fancy balloon*/
            FolderRepoInfo fri = notificationList[currentNotificationIndex];
            if (fri != null)
                fancyBalloon.DataContext = fri;
        }

        /// <summary>
        /// Updates the text box text basing on elemtns count and current index
        /// </summary>
        static void UpdateCounterTextBox()
        {
            fancyBalloon.UpdateCounterTextBox(notificationList.Count, currentNotificationIndex + 1);
        }


        /// <summary>
        /// Handles the request of view changes updacoming from balloon 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void fancyBalloon_RepositoryChangesViewRequested(object sender, RepositoryRoutedEventArgs e)
        {
            AppCommands.GetRepoInfoCommand.Execute(e.RepositoryUrl);
        }


        /// <summary>
        /// Handles the request of update of the specified repository  cosen in balloon 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void fancyBalloon_RepositoryUpdateRequested(object sender, RepositoryRoutedEventArgs e)
        {
            AppCommands.UpdateRepositoryCommand.Execute(e.RepositoryUrl);
        }


    }
}
