using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SvnRadar.Util
{
    /// <summary>
    /// Defines SilentNotification content
    /// </summary>
    public class SilentNotification
    {
        /// <summary>
        /// Sepcified error code
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// Specified error description
        /// </summary>
        public string ErrorDescription { get; set; }


        public override bool Equals(object obj)
        {
            if (obj== null || obj is SilentNotification == false)
                return base.Equals(obj);
            else
            {
                return (((SilentNotification)obj).ErrorCode == this.ErrorCode);
            }
        }
    }
}
