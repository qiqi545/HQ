#region LICENSE
// Unless explicitly acquired and licensed from HQ.IO Corporation 
// under another license, the contents of this file are copyrighted, 
// and you are strictly prohibited from using it for any purpose. 
// Violations will be prosecuted under the full extent the law allows.
#endregion

using Blowdart.UI;
using Blowdart.UI.Web;
using Demo.UI.Shared;

namespace Demo.UI
{
    public class LogStream : Admin
    {
        public override void Content(Ui ui)
        {
            ui.BeginDiv(new { @class = "ui main text container", style = "margin-top: 7em;" });
            {
                ui.Component<LogStreamViewer>();
            }
            ui.EndDiv();
        }
    }
}
