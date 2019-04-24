﻿using System;
using Blowdart.UI;
using Blowdart.UI.Web;
using Blowdart.UI.Web.SemanticUI;

using static InlineElements;

namespace Demo
{
	// todo remove site.Title/site.System
	// todo chainable lowercase, i.e. input().br();
	// todo generate innerText/attr pathway
	// todo generate all indirection helpers (including qualified)
	// todo make UiSystem hierarchical (app => class => method)
	// todo implement Meta
	// todo named/configurable defaults for things like <form method='post'>?
	// todo prune literal inline element list?
	// todo auto-localization
	// todo remove need to capture Ui variable in IMGUI handlers
	// todo order of first few qualified variables should be usage based (i.e. class/style always first and second, etc.) before switching to alphabetic
	// todo remove need to call UiServer.Start explicitly?
	// todo conventional first/only/Default method handler scanning

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
        public static void Default(string host, string firstName, string lastName)
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
