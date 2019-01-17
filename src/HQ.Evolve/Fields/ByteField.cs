using System;
using System.Text;

namespace HQ.Evolve.Fields
{
    public readonly ref struct ByteField
    {
        public byte? Value => !_encoding.TryParse(_buffer, out byte value) ? default(byte?) : value;
        public string RawValue => _encoding.GetString(_buffer);

        private readonly Encoding _encoding;
        private readonly ReadOnlySpan<byte> _buffer;

        public ByteField(ReadOnlySpan<byte> buffer, Encoding encoding)
        {
            _buffer = buffer;
            _encoding = encoding;
        }

        public unsafe ByteField(byte* start, int length, Encoding encoding)
        {
            _buffer = new ReadOnlySpan<byte>(start, length);
            _encoding = encoding;
        }
    }
}
