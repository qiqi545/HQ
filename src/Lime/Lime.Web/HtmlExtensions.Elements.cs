using System;

namespace Lime.Web
{
	partial class HtmlExtensions
    {
	    public static Ui Form(this Ui ui, Action<FormAttributes> action)
	    {
		    return ui.Element("form", new { }, x =>
		    {
			    var attributes = new FormAttributes();
			    attributes.method = "post";
			    action(attributes);
		    });
	    }

	    public struct FormAttributes
	    {
		    public string method;
	    }
	}
}
