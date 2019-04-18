using System;

namespace Blowdart.UI
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HandlerAttribute : Attribute
    {
        public string Template { get; }
        public string Name { get; }
        
        public HandlerAttribute(string template, string name)
        {
            Template = template;
            Name = name;
        }
    }
}