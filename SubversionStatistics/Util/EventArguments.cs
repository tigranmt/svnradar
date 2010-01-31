using System;

namespace SubversionStatistics.Util
{
    public class ProcessErrorEventArgs : EventArgs
    {
        public string Message { get; set; }
        public int ProcessID { get; set; }
    }
}