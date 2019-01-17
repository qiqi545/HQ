namespace HQ.Evolve.Internal
{
    internal static class Constants
    {
        public static readonly byte[] WorkingBytes = new byte[WorkingBytesLength];

        public const byte CarriageReturn = (byte)'\r';
        public const byte LineFeed = (byte)'\n';
        public const string Comma = ",";
        public const string Tab = "\t";
        public const string Pipe = "|";

        public const int ReadAheadSize = 128;
        public const int PadSize = 4;
        public const int BlockSize = 4096;
        public const int WorkingBytesLength = ReadAheadSize + BlockSize + PadSize;
    }
}
