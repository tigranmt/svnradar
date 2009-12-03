using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using SvnRadar.Common.Intefaces;

namespace SvnRadar.Util
{
    public static class WindowsManager
    {
        static List<IRepoWindow> windowsList = new List<IRepoWindow>();
     
        /// <summary>
        /// Adds the specified RevisonInfoWindow object to collection
        /// </summary>
        /// <param name="wnd">Window object to add</param>
        public static void AddNewWindow(IRepoWindow wnd)
        {
            if (windowsList == null)
                return;

            windowsList.Add(wnd);

        }

        /// <summary>
        /// Notify all windows that are listenres of the specified process about that the process has exited.
        /// </summary>
        /// <param name="repositoryProcessName"></param>
        /// <param name="commandName">Associated command name</param>
        public static void NotifyRepositoryProcessExit(string repositoryProcessName, string commandName)
        {
            /*Find among all available windows present in applictio that are listeners of the specified process 
             and notify them about that the prcess has exited.*/
            Action<IRepoWindow> notifyAboutExitAction = ((x) =>
            {
                if (!string.IsNullOrEmpty(x.RelatedRepositoryName) &&  x.RelatedRepositoryName.Equals(repositoryProcessName, StringComparison.InvariantCultureIgnoreCase))
                    x.ProcessExited();
            });
            windowsList.ForEach(notifyAboutExitAction);
        }

        /// <summary>
        /// Removes all windoss available in application that are listeners of the specified proces's repositry name
        /// </summary>
        /// <param name="repositoryProcessName">Repository name of the process of interest</param>
        /// <param name="commandName">Associated command name</param>
        public static void RemoveWindow(string repositoryProcessName,string commandName)
        {
            if (windowsList == null || windowsList.Count == 0)
                return;

            int countRemoved =windowsList.RemoveAll((x) => x.Process != null && x.Process.RelatedRepositoryName.Equals(repositoryProcessName, StringComparison.InvariantCulture) && 
                x.RelatedCommand.Equals(commandName,StringComparison.InvariantCultureIgnoreCase));
        }


        /// <summary>
        /// Finds window in the collectin of available ones by specified repository complete path and command executed on that repository
        /// </summary>
        /// <param name="repositoryCompletePath">Repository complete path</param>
        /// <param name="commandName">Command name</param>
        /// <returns></returns>
        public static IRepoWindow FindWindow(string repositoryCompletePath, string commandName)
        {
            if (string.IsNullOrEmpty(repositoryCompletePath))
                return null;


            return windowsList.Find((x) => x.Process != null && x.Process.RelatedRepositoryName.Equals(repositoryCompletePath, StringComparison.InvariantCulture) &&
                x.RelatedCommand.Equals(commandName, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Removs the specifed RevisionInfoWindow object from the collection of available ones
        /// </summary>
        /// <param name="wnd">RevisionInfWindow</param>
        public static void RemoveWindow(IRepoWindow wnd)
        {
            if (windowsList == null || windowsList.Count == 0)
                return;

            /*First try to locatr an object in the collection*/
            int indexOf = windowsList.IndexOf(wnd);
            if (indexOf >= 0)
                windowsList.RemoveAt(indexOf);
            else
            {
                /*If the object is not the same for some reason, for example it was cloned, locate all 
                 objets to remove via internal properties equality comparison.*/
                if(wnd.Process != null) 
                {
                    RemoveWindow(wnd.Process.RelatedRepositoryName,wnd.RelatedCommand);
                }

            }

        }
    }
}
