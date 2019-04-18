using System;

namespace Blowdart.UI
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class MetaAttribute : Attribute
    {
        public string Name { get; }
        public string Content { get; }

        public MetaAttribute(string name, string content)
        {
            Name = name;
            Content = content;
        }
    }
}