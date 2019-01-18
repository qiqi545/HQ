using System;
using System.Text;

namespace HQ.Data.Streaming.Fields
{
    public readonly ref struct DateTimeField
    {
        public DateTime? Value => !_encoding.TryParse(_buffer, out DateTime value) ? default(DateTime?) : value;
        public string RawValue => _encoding.GetString(_buffer);

        private readonly Encoding _encoding;
        private readonly ReadOnlySpan<byte> _buffer;

        public DateTimeField(ReadOnlySpan<byte> buffer, Encoding encoding)
        {
            _buffer = buffer;
            _encoding = encoding;
        }

        public unsafe DateTimeField(byte* start, int length, Encoding encoding)
        {
            _buffer = new ReadOnlySpan<byte>(start, length);
            _encoding = encoding;
        }
    }
}
