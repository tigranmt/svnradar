using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hardcodet.Wpf.TaskbarNotification;
using RepoManager.Common.Controls;
using System.Threading;
using System.Windows.Threading;
using System.Windows;
using System.Collections.ObjectModel;


namespace RepoManager.Util
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
        static ObservableCollection<FolderRepoInfo> notificationList = new ObservableCollection<FolderRepoInfo>();

        /// <summary>
        /// Timer for itarating over the notificaion list
        /// </summary>
        /// 
        static System.Timers.Timer timer = new System.Timers.Timer();



        /// <summary>
        /// Timer for flip the icon on sys tray
        /// </summary>
        static System.Timers.Timer fliptimer = new System.Timers.Timer();


        /// <summary>
        /// Original sys tray icon
        /// </summary>
        static System.Drawing.Icon originalSysTrayIcon = null;


        /// <summary>
        /// Index of the current FolderRepoInfo object being processed in the list
        /// </summary>
        static int currentNotificationIndex = 0;


        static Stack<FolderRepoInfo> waitingInformation = new Stack<FolderRepoInfo>();
        #endregion





        #region ctor
        static TaskNotifierManager()
        {
            InitializeData();
        }
        #endregion

        #region Timer elapsed handler
        static void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (waitingInformation.Count == 0)
                return;

            FolderRepoInfo waitingToNotify = null;

            if (fancyBalloon.IsVisible)
                return;

            /*Pops the information from the waiting stack*/
            lock (waitingInformation)
            {
                waitingToNotify = waitingInformation.Pop();
            }

            if (waitingToNotify != null)
            {
                /*Get next bject in the notification list*/
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    ShowNotification(waitingToNotify);
                }));
            }



        }
        #endregion


        /// <summary>
        /// Pushes specified information on waiting stack, that will be processed by the timer
        /// </summary>
        /// <param name="info"></param>
        public static void PushInfoOnWait(FolderRepoInfo info)
        {
            waitingInformation.Push(info);
        }

        /// <summary>
        /// Initialize the data of the class
        /// </summary>
        public static void InitializeData()
        {
            /*Subscribing to collection change event , in order to notify to the user via balloon about new information available in repository */
            notificationList.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(notificationList_CollectionChanged);

            /*Initializing timer object*/
            timer = new System.Timers.Timer();
            timer.AutoReset = true;
            timer.Interval = 5000;

            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.Start();
        }




        #region notification list changes handler
        static void notificationList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            /*Notify to the user only if somethig was add to colleciton */

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {

            }
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
        /// Frees all allocated resource of the TaskNotifierManager
        /// </summary>
        public static void DestroyNotifier()
        {
            try
            {
                if (timer != null)
                {
                    timer.Stop();
                    timer.Dispose();
                }

                if (taskBarIcon != null && !taskBarIcon.IsDisposed)
                {
                    taskBarIcon.CloseBalloon();
                    taskBarIcon.Dispose();
                }

                if (notificationList != null)
                    notificationList.Clear();

            }
            finally
            {

            }
        }


        /// <summary>
        /// Shows first notification if exists
        /// </summary>
        public static void ShowFirstChangeIfThereIs()
        {
            if (notificationList == null || notificationList.Count == 0)
                return;

            ShowNotification(notificationList[0]);
        }


        /// <summary>
        /// Notifies to the ballon the list of the repositories that are up to date.
        /// </summary>
        /// <param name="listOfUpToDaterepositories">The list of repositories</param>
        public static void UpToDateRepositories(List<string> listOfUpToDaterepositories)
        {
            if (listOfUpToDaterepositories == null ||
                listOfUpToDaterepositories.Count == 0)
                return;

            /*need to update notiication list, by removing all that repositories that are up to date*/
            IEnumerable<FolderRepoInfo> repoInfoListToRemove = null; 
            lock (notificationList)
            {
                repoInfoListToRemove = notificationList.Where<FolderRepoInfo>((x) => listOfUpToDaterepositories.Contains(x.FolderPath));


                if (repoInfoListToRemove != null)
                {
                    foreach (FolderRepoInfo fri in repoInfoListToRemove.ToArray())
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

                    }
                }

               
            }

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
                if (fliptimer == null)
                    fliptimer = new System.Timers.Timer();
                else if (fliptimer.Enabled)
                    return;

                fliptimer.Elapsed += new System.Timers.ElapsedEventHandler(fliptimer_Elapsed);
                fliptimer.Interval = 800;
                fliptimer.Start();
            }
            else
            {
                if (fliptimer == null)
                    return;
                fliptimer.Stop();
                fliptimer.Elapsed -= new System.Timers.ElapsedEventHandler(fliptimer_Elapsed);
                fliptimer.Dispose();
                fliptimer = null;

                if (originalSysTrayIcon != null)
                    taskBarIcon.Icon = originalSysTrayIcon;

                notificationList.Clear();
            }



        }

        static void fliptimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (originalSysTrayIcon == null)
                originalSysTrayIcon = taskBarIcon.Icon;

            if (taskBarIcon.Icon == null)
                taskBarIcon.Icon = originalSysTrayIcon;
            else
                taskBarIcon.Icon = null;
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
                    ShowNotification(information);

                }


            }
            else
            {
                ///*Show the notification on system tray*/

                notificationList.Add(information);
               
                ///*Show the notification on system tray*/
                ShowNotification(information);
               
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
                PushInfoOnWait(information);
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
                return;

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
