using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Hardcodet.Wpf.TaskbarNotification;
using RepoManager;
using RepoManager.Util;

namespace RepoManager.Common.Controls
{
    /// <summary>
    /// Interaction logic for FancyBalloon.xaml
    /// </summary>
    public partial class FancyBalloon : UserControl
    {
        private bool isClosing = false;


        #region registering Routed events

        public delegate void RepositoryRoutedEventHandler(object sender, RepositoryRoutedEventArgs e);

        public static RoutedEvent RepositoryChangesViewRequestedEvent = EventManager.RegisterRoutedEvent("RepositoryChangesViewRequestedEvent",
            RoutingStrategy.Bubble, typeof(RepositoryRoutedEventHandler), typeof(FancyBalloon));

        public static RoutedEvent RepositoryUpdateRequestedEvent = EventManager.RegisterRoutedEvent("RepositoryUpdateRequestedEvent",
           RoutingStrategy.Bubble, typeof(RepositoryRoutedEventHandler), typeof(FancyBalloon));


        public static RoutedEvent NextChangeViewRequestEvent = EventManager.RegisterRoutedEvent("NextChangeViewRequestEvent",
          RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FancyBalloon));


        public static RoutedEvent PrevChangeViewRequestEvent = EventManager.RegisterRoutedEvent("PrevChangeViewRequestEvent",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FancyBalloon));


        public event RepositoryRoutedEventHandler RepositoryChangesViewRequested
        {
            add { AddHandler(RepositoryChangesViewRequestedEvent, value); }
            remove { RemoveHandler(RepositoryChangesViewRequestedEvent, value); }
        }

        public event RepositoryRoutedEventHandler RepositoryUpdateRequested
        {
            add { AddHandler(RepositoryUpdateRequestedEvent, value); }
            remove { RemoveHandler(RepositoryUpdateRequestedEvent, value); }
        }


        public event RoutedEventHandler NextChangeViewRequested
        {
            add { AddHandler(NextChangeViewRequestEvent, value); }
            remove { RemoveHandler(NextChangeViewRequestEvent, value); }
        }

        public event RoutedEventHandler PrevChangeViewRequested
        {
            add { AddHandler(PrevChangeViewRequestEvent, value); }
            remove { RemoveHandler(PrevChangeViewRequestEvent, value); }
        }
        #endregion


        #region BalloonText dependency property

        /// <summary>
        /// Description
        /// </summary>
        public static readonly DependencyProperty BalloonTextProperty =
            DependencyProperty.Register("BalloonText",
                                        typeof(string),
                                        typeof(FancyBalloon),
                                        new FrameworkPropertyMetadata(""));

        /// <summary>
        /// A property wrapper for the <see cref="BalloonTextProperty"/>
        /// dependency property:<br/>
        /// Description
        /// </summary>
        public string BalloonText
        {
            get { return (string)GetValue(BalloonTextProperty); }
            set { SetValue(BalloonTextProperty, value); }
        }


        #endregion



        #region Repository support properties
        /// <summary>
        /// Related repository complete path
        /// </summary>
        public string RepositoryCompletePath
        {
            get
            {
                FolderRepoInfo repoInfo = this.DataContext as FolderRepoInfo;
                if (repoInfo != null)
                    return repoInfo.FolderPath;

                return string.Empty;
            }
        }

        /// <summary>
        /// Repository Url
        /// </summary>
        public string Url
        {
            get
            {
                FolderRepoInfo repoInfo = this.DataContext as FolderRepoInfo;
                if (repoInfo != null)
                    return repoInfo.Url;

                return string.Empty;
            }
        }

        #endregion


        public FancyBalloon()
        {
            InitializeComponent();
            TaskbarIcon.AddBalloonClosingHandler(this, OnBalloonClosing);
        }


        /// <summary>
        /// By subscribing to the <see cref="TaskbarIcon.BalloonClosingEvent"/>
        /// and setting the "Handled" property to true, we suppress the popup
        /// from being closed in order to display the fade-out animation.
        /// </summary>
        private void OnBalloonClosing(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            isClosing = true;
        }



        /// <summary>
        /// Updates content of the Counter Text box in order to show to the user how much records are they and on which is he now.
        /// </summary>
        /// <param name="totalCount">Total records count</param>
        /// <param name="currentIndex">Current record index</param>
        public void UpdateCounterTextBox(int totalCount, int currentIndex)
        {
            lock (CounterTextBox)
            {
                CounterTextBox.Text = currentIndex.ToString() + " : " + totalCount.ToString();
            }
        }

        /// <summary>
        /// Resolves the <see cref="TaskbarIcon"/> that displayed
        /// the balloon and requests a close action.
        /// </summary>
        private void imgClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //the tray icon assigned this attached property to simplify access
            TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
            taskbarIcon.CloseBalloon();
        }

        /// <summary>
        /// If the users hovers over the balloon, we don't close it.
        /// </summary>
        private void grid_MouseEnter(object sender, MouseEventArgs e)
        {
            //if we're already running the fade-out animation, do not interrupt anymore
            //(makes things too complicated for the sample)
            if (isClosing) return;

            //the tray icon assigned this attached property to simplify access
            TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
            taskbarIcon.ResetBalloonCloseTimer();
        }


        /// <summary>
        /// Closes the popup once the fade-out animation completed.
        /// The animation was triggered in XAML through the attached
        /// BalloonClosing event.
        /// </summary>
        private void OnFadeOutCompleted(object sender, EventArgs e)
        {
            Popup pp = (Popup)Parent;
            pp.IsOpen = false;
        }

        private void ButtonUpdateAll_Click(object sender, RoutedEventArgs e)
        {
            /*Execute update command on the related repository*/
            RaiseUpdateRepoEvent(RepositoryCompletePath);

        }

        private void ButtonViewLog_Click(object sender, RoutedEventArgs e)
        {
            /*Get log information*/
            RaiseViewChangeLogEvent(RepositoryCompletePath);
        }


        /// <summary>
        /// Raises notification of use Prev change notification button click
        /// </summary>
        private void RaiseViewPrevChangeEvent()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = PrevChangeViewRequestEvent;
            RoutedEventHelper.RaiseEvent(this, args);
        }

        /// <summary>
        /// Raises notification of use Next change notification button click
        /// </summary>
        private void RaiseViewNextChangeEvent()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = NextChangeViewRequestEvent;
            RoutedEventHelper.RaiseEvent(this, args);
        }

        /// <summary>
        /// Raises ChangeView Log event that will be handleed by the main applicaiton window
        /// </summary>
        /// <param name="repoUrl">Repository complete Url</param>
        private void RaiseViewChangeLogEvent(string repoUrl)
        {
            RepositoryRoutedEventArgs args = new RepositoryRoutedEventArgs();
            args.RepositoryUrl = repoUrl;
            args.RoutedEvent = RepositoryChangesViewRequestedEvent;
            RoutedEventHelper.RaiseEvent(this, args);
        }


        /// <summary>
        /// Raises Update all event that be handled by the main applicaiton window
        /// </summary>
        /// <param name="repoUrl">Repository complete Url</param>
        private void RaiseUpdateRepoEvent(string repoUrl)
        {
            RepositoryRoutedEventArgs args = new RepositoryRoutedEventArgs();
            args.RepositoryUrl = repoUrl;
            args.RoutedEvent = RepositoryUpdateRequestedEvent;
            RoutedEventHelper.RaiseEvent(this, args);
        }

        private void ButtonPrev_Click(object sender, RoutedEventArgs e)
        {
            RaiseViewPrevChangeEvent();
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            RaiseViewNextChangeEvent();
        }


        /// <summary>
        /// Enables/Disables naviagtion buttons on balloon
        /// </summary>
        /// <param name="enable">True to enable the navigation buttons, False otherwise</param>        
        public void EnableRecordNavigation(bool enable)
        {
            if (enable)
            {
                ButtonNext.Visibility = Visibility.Visible;
                ButtonPrev.Visibility = Visibility.Visible;
            }
            else
            {
                ButtonNext.Visibility = Visibility.Hidden;
                ButtonPrev.Visibility = Visibility.Hidden;
            }
        }



    }
}
