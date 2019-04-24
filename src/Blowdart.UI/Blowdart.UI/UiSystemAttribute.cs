using System;

namespace Blowdart.UI
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class UiSystemAttribute : Attribute
    {
        public Type Type { get; }

        public UiSystemAttribute(Type type)
        {
            Type = type;
        }
    }
}