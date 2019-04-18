using System;

namespace Blowdart.UI
{
    [AttributeUsage(AttributeTargets.Method)]
    public class UiSystemAttribute : Attribute
    {
        public Type Type { get; }

        public UiSystemAttribute(Type type)
        {
            Type = type;
        }
    }
}