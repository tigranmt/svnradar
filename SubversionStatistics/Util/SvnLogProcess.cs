using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SubversionStatistics.Util
{
    public class SvnLogProcess : Process, IDisposable
    {
        #region fields 
        /// <summary>
        /// Xml string buffer
        /// </summary>
        StringBuilder xmlData = new StringBuilder();

        /// <summary>
        /// If True, the object is now in string buffer populating stage, False otherwise
        /// </summary>
        public bool StartPopulating { get; set; }
        #endregion 

        #region methods
        /// <summary>
        /// Appends specified string to the buffer
        /// </summary>
        /// <param name="xmlString"></param>
        public void AppendXmlData(string xmlString)
        {
            if (string.IsNullOrEmpty(xmlString))
                return;
            xmlData.Append(xmlString);
        }      

        /// <summary>
        /// Gets the string buffer
        /// </summary>
        /// <returns></returns>
        public StringBuilder GetXmlString()
        {
            return xmlData;
        }

        /// <summary>
        /// Clears data 
        /// </summary>
        public void ClearData()
        {
            if(xmlData != null)
                xmlData.Remove(0, xmlData.Length);           
        }


        #endregion      

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            if (xmlData != null)
                xmlData.Remove(0, xmlData.Length);
            xmlData = null;
        }

        #endregion
    }
}
