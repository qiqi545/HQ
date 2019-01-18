using System;
using System.Text;

namespace HQ.Data.Streaming.Fields
{
    public readonly ref struct Int16Field
    {
        public short? Value => !_encoding.TryParse(_buffer, out short value) ? default(short?) : value;
        public string RawValue => _encoding.GetString(_buffer);

        private readonly Encoding _encoding;
        private readonly ReadOnlySpan<byte> _buffer;

        public Int16Field(ReadOnlySpan<byte> buffer, Encoding encoding)
        {
            _buffer = buffer;
            _encoding = encoding;
        }

        public unsafe Int16Field(byte* start, int length, Encoding encoding)
        {
            _buffer = new ReadOnlySpan<byte>(start, length);
            _encoding = encoding;
        }
    }
}
