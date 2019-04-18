using System;

namespace Blowdart.UI
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HandlerNameAttribute : Attribute
    {
        public string Name { get; }

        public HandlerNameAttribute(string name)
        {
            Name = name;
        }
    }
}