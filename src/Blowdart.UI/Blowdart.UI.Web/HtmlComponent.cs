#region LICENSE
// Unless explicitly acquired and licensed from HQ.IO Corporation 
// under another license, the contents of this file are copyrighted, 
// and you are strictly prohibited from using it for any purpose. 
// Violations will be prosecuted under the full extent the law allows.
#endregion
namespace Blowdart.UI.Web
{
    public abstract class HtmlComponent : UiComponent
    {
        public static Attributes Attr(object attr)
        {
            return Attributes.Attr(attr);
        }
    }
}