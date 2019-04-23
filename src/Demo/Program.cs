using System;
using Blowdart.UI;
using Blowdart.UI.Web;
using Blowdart.UI.Web.SemanticUI;

using static InlineElements;

namespace Demo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            UiConfig.Settings = site =>
            {
                site.Title = "Demo";
                site.System = new SemanticUi();
            };
            UiServer.AddHandler("/", "Home");
            UiServer.Start(args);
        }

        [HandlerName("Home"), SemanticUi, Meta("title", "Demo")]
        public static void DefaultPage(Ui ui, string host, string firstName, string lastName)
        {
            ui.P($"Hello, World from {strong(host)}!");

            ui.Form(new { method = "post" }, () =>
            {
                ui.Fieldset(() =>
                {
                    ui.Literal("First name: ").Break();
                    ui.Input(InputType.Text, new { name = "firstname", value = firstName, placeholder = "Enter your first name:" }).Break();
                    ui.Literal("Last name: ").Break();
                    ui.Input(InputType.Text, new { name = "lastname", value = lastName, placeholder = "Enter your last name:" }).Break();
                    ui.Submit("Post to Server");
                });
            });

            br();
			
            if (ui.Button("Click Me"))
            {
                Console.WriteLine($"Clicked By {firstName} {lastName}!");
            }
        }
    }
}
