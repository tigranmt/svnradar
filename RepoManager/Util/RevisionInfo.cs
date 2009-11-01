using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Documents;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;


namespace RepoManager.Util
{
    /// <summary>
    /// Class for holding revision related information 
    /// </summary>
    public sealed class RevisionInfo: INotifyPropertyChanged
    {



        /// <summary>
        /// Dependency property of FlowDocument object
        /// </summary>
        public static readonly System.Windows.DependencyProperty TextChangedProperty = DependencyProperty.Register("TextChanged", typeof(string), typeof(RevisionInfo));

       

        #region ctor 
        public RevisionInfo() 
        { 
        }
        #endregion


        /*Paragraph for made changes text output*/
        //Paragraph para = new Paragraph();

        /// <summary>
        /// Event for implementing INotifyPropertyChanged interface
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;




        /// <summary>
        /// Date of revision
        /// </summary>
        public string Date { get; set; }


        /// <summary>
        /// Revision number
        /// </summary>
        public int Revision {get;set;} 


        /// <summary>
        /// Text commented during commit on this revision if there is
        /// </summary>
        public List<string> ChangesMade { get; set; }


        /// <summary>
        /// Clears internal data
        /// </summary>
        public void ClearData()
        {
            ChangesMade.Clear();
           // para = null;

        }

        /// <summary>
        /// Appends to the string the Text property assigned text
        /// </summary>
        public string TextChanged
        {
            set
            {
                if (ChangesMade == null)
                    ChangesMade = new List<string>();
                ChangesMade.Add(value);


                OnPropertyChanged("TextChanged");
            }


        }

        /// <summary>
        /// File changed by this revision
        /// </summary>
        public string Item { get; set; }



        /// <summary>
        /// Parses the given string into the convinient Rich text format
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public TextBlock ParseMadeChangesToBlock()
        {
            if (ChangesMade == null || ChangesMade.Count == 0)
                return null;

            return ParseLineToBlock(ChangesMade[ChangesMade.Count - 1]);
        }

        /// <summary>
        /// Parses single line of a content to the TextBlock control
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        TextBlock ParseLineToBlock(string line)
        {
            try
            {
                if (string.IsNullOrEmpty(line))
                    return null;

                
                if (line.StartsWith("Index:") || line.IndexOf(this.Item, StringComparison.InvariantCultureIgnoreCase) > 0)
                {
                    TextBlock tb = new TextBlock();
                    tb.Text = line;
                    tb.Foreground = System.Windows.Media.Brushes.Brown;
                    
                    return tb;
                }
                else if (line.StartsWith("+"))
                {
                    TextBlock tb = new TextBlock();
                    tb.Text = line;
                    tb.Foreground = System.Windows.Media.Brushes.LimeGreen;
                    return tb;
                }
                else if (line.StartsWith("-"))
                {
                    TextBlock tb = new TextBlock();
                    tb.Text = line;
                    tb.Foreground = System.Windows.Media.Brushes.Red;
                    return tb;
                }
            }
            catch
            {
                return new TextBlock();
            }

            TextBlock defaultTb = new TextBlock();
            defaultTb.Text = line;
            defaultTb.Foreground = System.Windows.Media.Brushes.Black;

            return defaultTb;
        }


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

        /// <summary>
        /// Retruns the User comment related to the current revison
        /// </summary>
        public string UserComment
        {
            get { return RepoInfo.GetRevisionUserComment(this.Revision); }
        }

    }
}
