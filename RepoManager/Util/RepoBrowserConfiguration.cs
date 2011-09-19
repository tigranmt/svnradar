/* RepoBrowserConfiguration.cs --------------------------------
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
using System.Windows.Media;
using System.Runtime.Serialization;
using System.IO;
using System.Windows;
using System.Reflection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Xml;
using System.Xml.Serialization;

namespace SvnRadar.Util
{
     
    public sealed class RepoBrowserConfiguration : INotifyPropertyChanged
    {

        #region fields


        /// <summary>
        /// Object for saving/loading configuration data
        /// </summary>
        static RepoBrowserConfigurationModel configurationModel = null;

        /// <summary>
        /// Instance object
        /// </summary>
        static RepoBrowserConfiguration browser = null;

        static readonly string BatchFileDefaultContent = "@echo OFF "+ System.Environment.NewLine +
        " rem Configure WinMerge. " + System.Environment.NewLine + 
        " set DIFF=\"{0}\"" +  System.Environment.NewLine + 
        " set LEFT_TITLE=%3 "  +  System.Environment.NewLine + 
        " set RIGHT_TITLE=%5 " +  System.Environment.NewLine + 
        " set LEFT=%6 "  +  System.Environment.NewLine + 
        " set RIGHT=%7 "  +  System.Environment.NewLine + 
        " %DIFF%    /e /ub /dl %LEFT_TITLE% /dr %RIGHT_TITLE%   %LEFT%  %RIGHT%";






        #endregion

        #region ctor
        /*Private constructor*/
        public RepoBrowserConfiguration() { configurationModel = new RepoBrowserConfigurationModel(); }
        #endregion

       

        #region properties
        /// <summary>
        /// Defines ListView current layout
        /// </summary>       
        public SvnRadar.Util.RepoBrowserConfigurationModel.ListViewLayoutEnum ViewLayout 
        {
            get
            {
                return configurationModel.ViewLayout;
            }
            set 
            {
                configurationModel.ViewLayout = value;
                OnPropertyChanged("ViewLayout");
            }
        }




        public RepoBrowserConfigurationModel Model
        {
            get { return configurationModel; }
        }

        public static RepoBrowserConfiguration Instance
        {
            get
            {
                if (browser == null) { 
                    //browser = new RepoBrowserConfiguration();
                    browser = (RepoBrowserConfiguration)((ObjectDataProvider)AppResourceManager.FindResource("Configuration")).ObjectInstance;
               }
                return browser;
            }
        }




        /// <summary>
        /// True if the WinMerge path defined
        /// </summary>
        public bool IsWinMergeDefined
        {
            get
            {
                if (string.IsNullOrEmpty(WinMergePath))
                    return false;

                if (!File.Exists(WinMergePath))
                    return false;


                return true;
            }
        }



        /// <summary>
        /// Subversion exe file complete path
        /// </summary>       
        public string SubversionPath
        {
            get { return configurationModel.SubversionPath; }
            set { configurationModel.SubversionPath = value; }
        }


        /// <summary>
        /// WinMerge external tool  exe file complete path
        /// </summary>      
        public string WinMergePath 
        {
            get { return configurationModel.WinMergePath; }
            set { configurationModel.WinMergePath = value; }
        }


        /// <summary>
        /// Client repository path
        /// </summary>      
        public ObservableCollection<Repository> RepositoryPaths
        {
            get
            {
                return  configurationModel.RepositoryPaths;

            }
        }



        /// <summary>
        /// Tortoise exe file complete path
        /// </summary>        
        public string TortoisePath 
        {
            get { return configurationModel.TortoisePath; }
            set { configurationModel.TortoisePath = value; }
        }

        /// <summary>
        /// Minutes rate to cotrol on repository
        /// </summary>        
        public int ControlRate 
        {
            get { return configurationModel.ControlRate; }
            set { configurationModel.ControlRate = value; }
        }




        /// <summary>
        /// If true program will run on Startup
        /// </summary>       
        public bool RunOnStartUp 
        { 
            get 
            {
                return configurationModel.RunOnStartUp;
            }
            set
            {
                configurationModel.RunOnStartUp = value;
                OnPropertyChanged("RunOnStartUp");
            }
        
        }




        public bool IsSubversionOk
        {
            get { return !string.IsNullOrEmpty(SubversionPath); }
        }


        public bool IsTortoiseOk
        {
            get { return !string.IsNullOrEmpty(TortoisePath); }
        }

        #endregion

        #region member functions
      

        #region add/remove repository path methods

        /// <summary>
        /// Add repository path to the collection
        /// </summary>
        /// <param name="repoPath">The path to add to collection</param>
        public void AddRepoPath(string repoPath)
        {
            if (!RepositoryPaths.Any((x) => x.RepositoryCompletePath.Equals(repoPath, StringComparison.InvariantCultureIgnoreCase)))
            {
                Repository repo = new Repository { RepositoryCompletePath = repoPath };
                RepositoryPaths.Add(repo);
            }
        }
        


        /// <summary>
        /// Remove repository path to the collection
        /// </summary>
        /// <param name="repoPath">The path to remove from the collection</param>
        public void RemoveRepoPath(string repoPath)
        {
            IEnumerable<Repository> repoList = RepositoryPaths.Where((x)=>x.RepositoryCompletePath.Equals(repoPath,StringComparison.InvariantCultureIgnoreCase));
            if (repoList != null)
            {
                foreach (Repository r in repoList)
                {
                    RepositoryPaths.Remove(r);
                    break;
                }
            }

        }

        #endregion

 
        #region batch methodds

        /// <summary>
        /// Generate BatchFile content. If the WinMerge path is empty or not correct the result will be an empty string.
        /// </summary>
        /// <returns>Returns the content of batch file</returns>
        public string GenerateBatchFileContent()
        {
            if (string.IsNullOrEmpty(WinMergePath) ||
                !File.Exists(WinMergePath))
                return string.Empty;

            return string.Format(BatchFileDefaultContent, WinMergePath);
        }

        #endregion

        #region set alias method
        /// <summary>
        /// Assign specified alias to the specified repository in the internal colelciton of Repository objects
        /// </summary>
        /// <param name="repositoryPath">Repository complete path</param>
        /// <param name="repositoryAlias">Repository new alias</param>
        public void AssignAliasToTab(string repositoryPath, string repositoryAlias)
        {
            IEnumerable<Repository> repoList = RepositoryPaths.Where((x) => x.RepositoryCompletePath.Equals(repositoryPath, StringComparison.InvariantCultureIgnoreCase));
            if (repoList != null)
            {
                foreach (Repository r in repoList)
                {
                    r.UserAssignedAlias = repositoryAlias;
                    break;
                }
            }
        }

        /// <summary>
        /// Gets the respository assigned alias if there is any
        /// </summary>
        /// <param name="repositoryPath">Repository complete path</param>
        /// <returns></returns>
        public string GetAliasForRepository(string repositoryPath)
        {
            string alias = string.Empty;
            IEnumerable<Repository> repoList = RepositoryPaths.Where((x) => x.RepositoryCompletePath.Equals(repositoryPath, StringComparison.InvariantCultureIgnoreCase));
            if (repoList != null)
            {
                foreach (Repository r in repoList)
                {
                    alias = r.UserAssignedAlias;
                    break;
                }
            }


            return alias;
        }
        #endregion
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
