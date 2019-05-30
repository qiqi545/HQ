using System;
using Lime;
using Lime.Web;
using Lime.Web.SemanticUi;

using static Lime.Web.InlineElements;

namespace Demo
{
	[DeployOn(DeployTarget.Client)]
	[SemanticUi, Title("Lime UI Demo")]
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
			 
            if(range(1, 100, 50) > 80)
            {
				button("Secret Button", (e, a) =>
	            {
		            e.click = d => Console.WriteLine($"Clicked By {firstName} {lastName}!");
		            e.mouseover = d => a.innerText = "Secret Button Hovered";
		            e.mouseout = d => a.innerText = "Secret Button Left";
	            });
            }
		}
    }
}