using System;
using System.Text;

namespace HQ.Evolve.Fields
{
    public readonly ref struct StringField
    {
        public string Value => RawValue;

        private readonly Encoding _encoding;
        private readonly ReadOnlySpan<byte> _buffer;

        public string RawValue => _encoding.GetString(_buffer);

        public StringField(ReadOnlySpan<byte> buffer, Encoding encoding)
        {
            _buffer = buffer;
            _encoding = encoding;
        }

        public unsafe StringField(byte* start, int length, Encoding encoding)
        {
            _buffer = new ReadOnlySpan<byte>(start, length);
            _encoding = encoding;
        }
    }
}
