using HQ.Installer;

namespace HQ.Template
{
    public class Program
    {
        public static void Main(string[] args)
        {
            HqServer.Start<Startup>(args);
        }
    }
}
