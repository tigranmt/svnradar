using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace SvnRadar.Util
{
    /// <summary>
    /// Class responsable for saving and loading configuration data
    /// </summary>
    [DataContractAttribute(Name = "Configuration")]
    public class RepoBrowserConfigurationModel
    {

        public RepoBrowserConfigurationModel() { }

        /// <summary>
        /// Configuration file name 
        /// </summary>
        static readonly string CONFIG_FILE_NAME = "config.dat";

        public static readonly string BATCH_FILE_NAME = "diff.bat";

        #region List view layut enumration declaration
        /// <summary>
        /// Defines the possible values of the alyout of the ListView items.
        /// GroupView: item are grouped by Revision number
        /// FlatView    : items are inserted just in sequence
        /// </summary>        
        public enum ListViewLayoutEnum { FlatView, GroupView };
        #endregion


        /// <summary>
        /// True, so run on Startup of OS, False otherwise
        /// </summary>
        bool bRunOnStartUp = true;

        /// <summary>
        /// List view initial layout.
        /// </summary>
        ListViewLayoutEnum listViewLayout = ListViewLayoutEnum.GroupView;

        /// <summary>
        /// Repository paths collection
        /// </summary>
        ObservableCollection<Repository> repoPaths = new ObservableCollection<Repository>();

        List<string> statDbPaths = new List<string>();

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
            }
        }


        /// <summary>
        /// Subversion exe file complete path
        /// </summary>       
        [DataMember]
        public string SubversionPath { get; set; }


        /// <summary>
        /// WinMerge external tool  exe file complete path
        /// </summary>      
       [DataMember]
        public string WinMergePath { get; set; }

       [DataMember]
        public string TortoisePath { get; set; }


        /// <summary>
        /// Repositories path
        /// </summary>
        [DataMember]
        public ObservableCollection<Repository> RepositoryPaths
        {

            get
            {
                if (repoPaths == null)
                    repoPaths = new ObservableCollection<Repository>();
                return repoPaths;

            }
        }
        
        [DataMember]
        public List<string> StatDbPaths
        {
            get { return statDbPaths; }
        }


        /// <summary>
        /// Minutes rate to cotrol on repository
        /// </summary>       
        [DataMember]
        public int ControlRate { get; set; }




        /// <summary>
        /// If true program will run on Startup
        /// </summary>     
        [DataMember]
        public bool RunOnStartUp
        {
            get
            {
                return bRunOnStartUp;
            }
            set
            {
                bRunOnStartUp = value;
            }

        }

        public void Save()
        {
            if (this.RunOnStartUp)
                InstallMeOnStartUp();
            else
                UnInstallMeOnStartUp();

            using (FileStream fs = new FileStream(ConfigFileCompletePath, FileMode.Create))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(RepoBrowserConfigurationModel));
                serializer.WriteObject(fs, this);
                fs.Close();

            }
        }



        #region save and load methods
        /// <summary>
        /// Save configuration to file
        /// </summary>


        /// <summary>
        /// Installs the porgramm on start up
        /// </summary>
        void InstallMeOnStartUp()
        {
            try
            {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                Assembly curAssembly = Assembly.GetExecutingAssembly();
                key.SetValue(curAssembly.GetName().Name, curAssembly.Location);
            }
            catch (Exception ex)
            {
                ErrorManager.ShowExceptionError(ex, true);
            }
        }

        /// <summary>
        /// Unistalls program from startup
        /// </summary>
        void UnInstallMeOnStartUp()
        {
            try
            {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                Assembly curAssembly = Assembly.GetExecutingAssembly();
                key.DeleteValue(curAssembly.GetName().Name, false);
            }
            catch (Exception ex)
            {
                ErrorManager.ShowExceptionError(ex, true);
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
                File.Copy(ConfigFileCompletePath, fileCompletePath, true);
            }
            catch (Exception ex)
            {
                ErrorManager.ShowExceptionError(ex, true);
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
                        File.Copy(fileCompletePath, ConfigFileCompletePath, true);

                        /*Load configuration*/
                        Load();
                    }
                    catch (Exception ex)
                    {
                        RestoreCurrentFile();
                        ErrorManager.LogException(ex);
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
                ErrorManager.ShowExceptionError(ex, true);
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

                if (File.Exists(backupFilePath))
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
        /// True if the file exists, so diff will be manged by the external WinMerge program, False otherwise
        /// </summary>
        public bool IsBatchFileExists
        {
            get
            {
                if (string.IsNullOrEmpty(BatchFileCompletePath))
                    return false;

                if (!File.Exists(BatchFileCompletePath))
                    return false;


                return true;
            }
        }

        /// <summary>
        /// Contains complete path of the batch file present on the client machine's isolated storage folder
        /// </summary>
        public static string BatchFileCompletePath
        {
            get
            {
                try
                {
                    string asemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    if (!string.IsNullOrEmpty(asemblyPath))
                    {
                        string assemblyDir = System.IO.Path.GetDirectoryName(asemblyPath);
                        if (!string.IsNullOrEmpty(assemblyDir))
                        {
                            return assemblyDir + System.IO.Path.DirectorySeparatorChar + BATCH_FILE_NAME;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorManager.LogException(ex);
                }


                return string.Empty;
            }

        }

        static string ConfigFileCompletePath
        {
            get
            {
                return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
                + System.IO.Path.DirectorySeparatorChar +
                    CONFIG_FILE_NAME;
            }
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
                bool allok = true;
                DataContractSerializer serializer = new DataContractSerializer(typeof(RepoBrowserConfigurationModel));
                try
                {
                    RepoBrowserConfigurationModel config = serializer.ReadObject(fs) as RepoBrowserConfigurationModel;
                    InitFromOther(config);
                }
                catch (SerializationException serEx)
                {
                    allok = false;
                    ErrorManager.ShowCommonError(AppResourceManager.FindResource("MSG_DESIRIALIZATIONEXCEPTION") as string, true);
                    ErrorManager.LogException(serEx);
                }
                catch (Exception ex)
                {
                    allok = false;
                    ErrorManager.ShowCommonError(AppResourceManager.FindResource("MSG_DESIRIALIZATIONEXCEPTION") as string, true);
                    ErrorManager.LogException(ex);
                }
                finally
                {

                    
                    if (!allok)
                    {
                        if (fs != null) 
                        {
                            fs.Close();
                            fs.Dispose();
                        }

                        string bkFile = Path.ChangeExtension(fPath, "backup");
                        if (File.Exists(bkFile))
                            File.Delete(bkFile);
                        File.Move(fPath, bkFile);
                    }
                }
            }



        }

        private void InitFromOther(RepoBrowserConfigurationModel other)
        {
            this.ControlRate = other.ControlRate;
            if (other.RepositoryPaths!=null)
                this.repoPaths = new ObservableCollection<Repository>(other.RepositoryPaths);

            this.RunOnStartUp = other.RunOnStartUp;

            if (other.StatDbPaths!=null)
                this.statDbPaths = new List<string>(other.StatDbPaths);

            this.SubversionPath = other.SubversionPath;
            this.TortoisePath = other.TortoisePath;
            this.ViewLayout = other.ViewLayout;
            this.WinMergePath = other.WinMergePath;            

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
            this.WinMergePath = string.Empty;

            try
            {
                if (File.Exists(BatchFileCompletePath))
                    File.Delete(BatchFileCompletePath);
            }
            catch (Exception ex)
            {
                ErrorManager.LogException(ex);
            }

            ViewLayout = ListViewLayoutEnum.FlatView;

        }



        #endregion



    }
}
