using System;
using System.Text;

namespace HQ.Data.Streaming.Fields
{
    public readonly ref struct FloatField
    {
        public float? Value => !_encoding.TryParse(_buffer, out float value) ? default(float?) : value;
        public string RawValue => _encoding.GetString(_buffer);

        private readonly Encoding _encoding;
        private readonly ReadOnlySpan<byte> _buffer;

        public FloatField(ReadOnlySpan<byte> buffer, Encoding encoding)
        {
            _buffer = buffer;
            _encoding = encoding;
        }

        public unsafe FloatField(byte* start, int length, Encoding encoding)
        {
            _buffer = new ReadOnlySpan<byte>(start, length);
            _encoding = encoding;
        }
    }
}
