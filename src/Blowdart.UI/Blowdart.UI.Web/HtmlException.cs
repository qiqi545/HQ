#region LICENSE
// Unless explicitly acquired and licensed from HQ.IO Corporation 
// under another license, the contents of this file are copyrighted, 
// and you are strictly prohibited from using it for any purpose. 
// Violations will be prosecuted under the full extent the law allows.
#endregion

using System;

namespace Blowdart.UI.Web
{
    public class HtmlException : Exception
    { 
        public HtmlException(string message) : base(message) { }
    }
}