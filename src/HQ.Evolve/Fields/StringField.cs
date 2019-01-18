using System;
using System.Text;

namespace HQ.Evolve.Fields
{
    public readonly ref struct StringField
    {
        public bool Initialized => _buffer != null;
        public string Value => RawValue;

        private readonly unsafe byte* _start;
        private readonly int _length;
        private readonly Encoding _encoding;
        private readonly ReadOnlySpan<byte> _buffer;

        public string RawValue => _encoding.GetString(_buffer);

        public unsafe StringField(byte* start, int length, Encoding encoding)
        {
            _buffer = new ReadOnlySpan<byte>(start, length);
            _start = start;
            _length = length;
            _encoding = encoding;
        }

        public unsafe StringField AddLength(int length)
        {
            return new StringField(_start, _length + length, _encoding);
        }
    }
}
