/* IRepoWindow.cs --------------------------------
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

namespace SvnRadar.Common.Intefaces
{
    /// <summary>
    /// Common interface that every window in application that handles the repository command output
    ///is implement
    /// </summary>
    public interface IRepoWindow
    {
        /// <summary>
        /// Process object associated to window
        /// </summary>
        RepositoryProcess Process { get; set; }

        /// <summary>
        /// Related repository name
        /// </summary>
        string RelatedRepositoryName { get; set; }


        /// <summary>
        /// Related command executed ober repository
        /// </summary>
        string RelatedCommand { get; set; }

        /// <summary>
        /// Notifies the widnow abou relative process exit
        /// </summary>
        void ProcessExited();


    }
}
