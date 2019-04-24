using System;
using Blowdart.UI;
using Blowdart.UI.Web;
using Blowdart.UI.Web.SemanticUI;

using static InlineElements;

namespace Demo
{
	// todo chainable lowercase, i.e. input().br();
	// todo generate innerText/attr pathway
	// todo generate all indirection helpers (including qualified)
	// todo named/configurable defaults for things like <form method='post'>?
	// todo prune literal inline element list?
	// todo auto-localization
	// todo order of first few qualified variables should be usage based (i.e. class/style always first and second, etc.) before switching to alphabetic
	// todo remove need to call UiServer.Start explicitly?

	[SemanticUi]
	[Meta("title", "Blowdart UI Demo")]
	internal class Program
    {
        private static void Main(string[] args)
        {
            UiServer.Start(args);
        }

        public static void Index(string host, string firstName, string lastName)
        {
            p($"Hello, World from {strong(host)}!");

            form(new { method = "post" }, () =>
            {
                fieldset(() =>
                {
	                literal("First name: ").Break();
                    input(InputType.Text, new { name = "firstname", value = firstName, placeholder = "Enter your first name:" }).Break();
                    literal("Last name: ").Break();
                    input(InputType.Text, new { name = "lastname", value = lastName, placeholder = "Enter your last name:" }).Break();
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