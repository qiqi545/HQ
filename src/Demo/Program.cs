using Blowdart.UI;
using Blowdart.UI.Web;
using Blowdart.UI.Web.SemanticUI;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            UiConfig.Settings = site =>
            {
                site.Title = "AntHill";
                site.System = new SemanticUi();
            };

            UiServer.Start(args, layout =>
            {
                layout.Default(ui =>
                {
                    ui.P("Hello, World!");
                });
            });
        }
    }
}
