using System;
using Blowdart.UI;
using Blowdart.UI.Web;
using Blowdart.UI.Web.SemanticUI;
using static InlineElements;

namespace Demo
{
	[Title("Blowdart UI Demo"), SemanticUi]
	internal class Program
    {
        private static void Main(string[] args) => UiServer.Start(args);

        public static void Index(string host, string firstName, string lastName)
        {
            p($"Hello, World from {strong(host)}!");

            form(x =>
            {
	            x.method = "post";

				fieldset(() =>
                {
	                _("First name: ").br();
                    input(InputType.Text, new { name = "firstname", value = firstName, placeholder = "Enter your first name:" }).br();
                    _("Last name: ").br();
                    input(InputType.Text, new { name = "lastname", value = lastName, placeholder = "Enter your last name:" }).br();
                    submit("Post to Server");
                });
            });

			br();

            if (button("Click Me"))
            {
                Console.WriteLine($"Clicked By {firstName} {lastName}!");
            }
        }
    }
}