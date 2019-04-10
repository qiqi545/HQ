using System;
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
                site.Title = "Demo";
                site.System = new SemanticUi();
            };

            UiServer.Start(args, layout =>
            {
                layout.Default(ui =>
                {
                    ui.P("Hello, World!");

                    if (ui.Button("Click Me!"))
                    {
                        Console.WriteLine("Foo");
                    }
                });
            });
        }
    }
}
