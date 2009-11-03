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

namespace SvnRadar.Util
{
    [DataContractAttribute(Name="ConfigDataContact")]
    internal sealed class RepoBrowserConfiguration : INotifyPropertyChanged
    {

        #region fields

        /// <summary>
        /// Configuration file name 
        /// </summary>
        static readonly string CONFIG_FILE_NAME = "confid.dat";

        /// <summary>
        /// List view initial layout.
        /// </summary>
        ListViewLayoutEnum listViewLayout = ListViewLayoutEnum.FlatView;

        /// <summary>
        /// Repository paths collection
        /// </summary>
        ObservableCollection<string> repoPaths = new ObservableCollection<string>();


        static RepoBrowserConfiguration browser = null;

        #endregion

        #region ctor
        /*Private constructor*/
        RepoBrowserConfiguration() { }
        #endregion

        #region List view layut enumration declaration
        /// <summary>
        /// Defines the possible values of the alyout of the ListView items.
        /// RevisionView: item are grouped by Revision number
        /// FlatView    : items are inserted just in sequence
        /// </summary>
        public enum ListViewLayoutEnum { FlatView, RevisionView };
        #endregion

        #region properties
        /// <summary>
        /// Defines ListView current layout
        /// </summary>
        [DataMember]
        public ListViewLayoutEnum ViewLayout 
        {
            get
            {
                return listViewLayout;
            }
            set 
            {
                listViewLayout = value;
                OnPropertyChanged("ViewLayout");
            }
        }




        public static RepoBrowserConfiguration Instance
        {
            get
            {
                if (browser == null)
                    browser = new RepoBrowserConfiguration();
                return browser;
            }
        }



        /// <summary>
        /// Subversion exe file complete path
        /// </summary>
        [DataMember()]
        public string SubversionPath { get; set; }



        /// <summary>
        /// Client repository path
        /// </summary>
        [DataMember()]
        public ObservableCollection<string> RepositoryPaths
        {

            get
            {
                if (repoPaths == null)
                    repoPaths = new ObservableCollection<string>();
                return repoPaths;

            }
        }



        /// <summary>
        /// Tortoise exe file complete path
        /// </summary>
        [DataMember()]
        public string TortoisePath { get; set; }

        /// <summary>
        /// Minutes rate to cotrol on repository
        /// </summary>
        [DataMember()]
        public int ControlRate { get; set; }



        static string ConfigFileCompletePath
        {
            get
            {
                return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
                + System.IO.Path.DirectorySeparatorChar +
                    CONFIG_FILE_NAME;
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
        #region save and load methods 
        /// <summary>
        /// Save configuration to file
        /// </summary>
        public void Save()
        {

            using (FileStream fs = new FileStream(ConfigFileCompletePath, FileMode.Create))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(RepoBrowserConfiguration));
                serializer.WriteObject(fs, Instance);
                fs.Close();

            }
        }



        /// <summary>
        /// Exports current configuration to the specified file path
        /// </summary>
        /// <param name="fileCompletePath">File complete path where the current configurations settings should be serialized</param>
        public void Export(string fileCompletePath)
        {
            if (string.IsNullOrEmpty(fileCompletePath))
                return;

            /*First of all save configuration*/
            Save();

            try
            {
                /*Copy just saved config file into specified location with specified name*/
                File.Copy(ConfigFileCompletePath, fileCompletePath,true);
            }
            catch (Exception ex)
            {
                ErrorManager.ShowExceptionError(ex,true);
            }


        }

        /// <summary>
        /// Imports configuration present in the specifed file into the system
        /// </summary>
        /// <param name="fileCompletePath">The complete file path where the configuration data located</param>
        public void Import(string fileCompletePath)
        {
            if (string.IsNullOrEmpty(fileCompletePath))
                return;

            if (!File.Exists(fileCompletePath))
                return;

            /*Backup current configuration in order if in case of an any exception let ot restre previouse settings*/
            if (!string.IsNullOrEmpty(ConfigFileCompletePath) &&
                File.Exists(ConfigFileCompletePath))
            {
                if (BackupCurrentFile())
                {
                    try
                    {
                        /*remove config file, we already backupped it */
                        File.Delete(ConfigFileCompletePath);

                        /*Move the file from the specified location to the current one*/
                        File.Copy(fileCompletePath, ConfigFileCompletePath,true);

                        /*Load configuration*/
                        Load();
                    }
                    catch (Exception ex)
                    {
                        RestoreCurrentFile();
                    }

                    CleanUp();
                    
                }
            }
        }


        /// <summary>
        /// Rstore previouse backuped file if exists
        /// </summary>
        /// <returns></returns>
        private bool RestoreCurrentFile() 
        {

            string originaleFileName = Path.GetFileNameWithoutExtension(ConfigFileCompletePath);
            string backupFileName = originaleFileName + ".bk";
            string curDirPath = Path.GetDirectoryName(ConfigFileCompletePath);
            string backupFilePath = curDirPath + Path.DirectorySeparatorChar + backupFileName;

            /* Do not do anything if backup file doesnt exist*/
            if (!File.Exists(backupFilePath))
                return false;

            try
            {
                /*delete old config file*/
                if (File.Exists(ConfigFileCompletePath))
                    File.Delete(ConfigFileCompletePath);

                /*Rename backup file */
                File.Copy(backupFilePath, ConfigFileCompletePath);


                /*remove backup file */
                if (File.Exists(backupFilePath))
                    File.Delete(backupFilePath);
            }
            catch (Exception ex)
            {
                ErrorManager.ShowExceptionError(ex, true);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Clean ups the directory from unenecessary temporary files
        /// </summary>
        /// <returns></returns>
        private bool CleanUp()
        {

            string originaleFileName = Path.GetFileNameWithoutExtension(ConfigFileCompletePath);
            string backupFileName = originaleFileName + ".bk";
            string curDirPath = Path.GetDirectoryName(ConfigFileCompletePath);
            string backupFilePath = curDirPath + Path.DirectorySeparatorChar + backupFileName;

            try
            {
                if (File.Exists(backupFilePath))
                    File.Delete(backupFilePath);
            }
            catch (Exception ex)
            {
                ErrorManager.ShowExceptionError(ex,true);
                return false;
            }


            return true;
        }

        /// <summary>
        /// Backups current configuration file
        /// </summary>
        /// <returns></returns>
        private bool BackupCurrentFile()
        {
            if (string.IsNullOrEmpty(ConfigFileCompletePath) ||
                !File.Exists(ConfigFileCompletePath))
                return false;


            string originaleFileName = Path.GetFileNameWithoutExtension(ConfigFileCompletePath);
            string backupFileName = originaleFileName + ".bk";
            string curDirPath = Path.GetDirectoryName(ConfigFileCompletePath);
            string backupFilePath = curDirPath + Path.DirectorySeparatorChar + backupFileName;

            try
            {

                if(File.Exists(backupFilePath)) 
                    File.Delete(backupFilePath);

                /*Backup current file */
                File.Copy(ConfigFileCompletePath, backupFilePath);

            }
            catch (Exception ex)
            {
                ErrorManager.ShowExceptionError(ex, true);
                return false;
            }


            return true;
        }

        /// <summary>
        /// Load configuration from file
        /// </summary>
        public void Load()
        {
            string fPath = ConfigFileCompletePath;
            if (!File.Exists(fPath))
            {
                ResetConfiguration();
                return;
            }

            using (FileStream fs = new FileStream(fPath, FileMode.Open))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(RepoBrowserConfiguration));
                try
                {
                    browser = serializer.ReadObject(fs) as RepoBrowserConfiguration;
                }
                catch (SerializationException serEx)
                {
                    ErrorManager.ShowCommonError(AppResourceManager.FindResource("MSG_DESIRIALIZATIONEXCEPTION") as string,true);
                    ErrorManager.LogException(serEx);

                }
            }


            
        }

        #endregion

        #region add/remove repository path methods

        /// <summary>
        /// Add repository path to the collection
        /// </summary>
        /// <param name="repoPath">The path to add to collection</param>
        public void AddRepoPath(string repoPath)
        {
            if (!RepositoryPaths.Contains(repoPath))
                RepositoryPaths.Add(repoPath);
        }


        /// <summary>
        /// Remove repository path to the collection
        /// </summary>
        /// <param name="repoPath">The path to remove from the collection</param>
        public void RemoveRepoPath(string repoPath)
        {
            if (RepositoryPaths.Contains(repoPath))
                RepositoryPaths.Remove(repoPath);

        }

        #endregion

        #region configuration methods 
        
        /// <summary>
        /// Resets coniguration to it's default values
        /// </summary>
        public void ResetConfiguration()
        {
            this.ControlRate = 1;
            this.RepositoryPaths.Clear();
            this.SubversionPath = string.Empty;
            this.TortoisePath = string.Empty;
            ViewLayout = ListViewLayoutEnum.FlatView;

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
