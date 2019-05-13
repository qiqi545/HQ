using System;

namespace Blowdart.UI
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HandlerAttribute : Attribute
    {
        public string Template { get; }
        
        public HandlerAttribute(string template)
        {
            Template = template;
        }
	}
}