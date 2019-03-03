using System.Diagnostics;

namespace HQ.Installer
{
    public static class Program
    {
        public static void Main(params string[] args)
        {
            MastHead();
        }

        public static void MastHead()
        {
            // Credit: http://patorjk.com/software/taag/
            Trace.TraceInformation(@"
 __   __  _______        ___   _______ 
|  | |  ||       |      |   | |       |
|  |_|  ||   _   |      |   | |   _   |
|       ||  | |  |      |   | |  | |  |
|       ||  |_|  | ___  |   | |  |_|  |
|   _   ||      | |   | |   | |       |
|__| |__||____||_||___| |___| |_______|
");
        }
    }
}
