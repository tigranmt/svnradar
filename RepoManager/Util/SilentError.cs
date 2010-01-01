/* SilentError.cs --------------------------------
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

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
