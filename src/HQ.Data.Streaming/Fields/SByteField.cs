using System;
using System.Text;

namespace HQ.Data.Streaming.Fields
{
    public readonly ref struct SByteField
    {
        public sbyte? Value => !_encoding.TryParse(_buffer, out sbyte value) ? default(sbyte?) : value;
        public string RawValue => _encoding.GetString(_buffer);

        private readonly Encoding _encoding;
        private readonly ReadOnlySpan<byte> _buffer;

        public SByteField(ReadOnlySpan<byte> buffer, Encoding encoding)
        {
            _buffer = buffer;
            _encoding = encoding;
        }

        public unsafe SByteField(byte* start, int length, Encoding encoding)
        {
            _buffer = new ReadOnlySpan<byte>(start, length);
            _encoding = encoding;
        }
    }
}
