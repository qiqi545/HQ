using System;
using System.Text;

namespace HQ.Evolve.Fields
{
    public readonly ref struct UInt32Field
    {
        public uint? Value => !_encoding.TryParse(_buffer, out uint value) ? default(uint?) : value;
        public string RawValue => _encoding.GetString(_buffer);

        private readonly Encoding _encoding;
        private readonly ReadOnlySpan<byte> _buffer;

        public UInt32Field(ReadOnlySpan<byte> buffer, Encoding encoding)
        {
            _buffer = buffer;
            _encoding = encoding;
        }

        public unsafe UInt32Field(byte* start, int length, Encoding encoding)
        {
            _buffer = new ReadOnlySpan<byte>(start, length);
            _encoding = encoding;
        }
    }
}
