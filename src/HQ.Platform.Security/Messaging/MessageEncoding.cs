using System.Text;

namespace HQ.Platform.Security.Messaging
{
    internal static class MessageEncoding
    {
        public static Encoding current = new UTF8Encoding(false, true);
    }
}
