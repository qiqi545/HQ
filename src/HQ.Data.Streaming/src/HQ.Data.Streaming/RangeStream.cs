#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HQ.Data.Streaming
{
    // Derived from MimeKit's BoundStream
    public class RangeStream : Stream
    {
        private readonly long _from;
        private readonly Stream _inner;
        private bool _endOfStream;

        private long _position;
        private long _to;

        public RangeStream(Stream inner, long from, long? to = null)
        {
            if (from < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(from));
            }

            if (to >= 0 && to < from)
            {
                throw new ArgumentOutOfRangeException(nameof(to));
            }

            _to = !to.HasValue || to.Value < 0 ? -1 : to.Value;
            _from = from;

            _inner = inner;
            _position = 0;
            _endOfStream = false;
        }

        public override bool CanRead => _inner.CanRead;

        public override bool CanWrite => _inner.CanWrite;

        public override bool CanSeek => _inner.CanSeek;

        public override bool CanTimeout => _inner.CanTimeout;

        public override long Length
        {
            get
            {
                if (_to != -1)
                {
                    return _to - _from;
                }

                if (_endOfStream)
                {
                    return _position;
                }

                return _inner.Length - _from;
            }
        }

        public override long Position
        {
            get => _position;
            set => Seek(value, SeekOrigin.Begin);
        }

        public override int ReadTimeout
        {
            get => _inner.ReadTimeout;
            set => _inner.ReadTimeout = value;
        }

        public override int WriteTimeout
        {
            get => _inner.WriteTimeout;
            set => _inner.WriteTimeout = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // if we are at the end of the stream, we cannot read anymore data
            if (_to != -1 && _from + _position >= _to)
            {
                _endOfStream = true;
                return 0;
            }

            // make sure that the source stream is in the expected position
            if (_inner.Position != _from + _position)
            {
                _inner.Seek(_from + _position, SeekOrigin.Begin);
            }

            var n = _to != -1 ? (int) Math.Min(_to - (_from + _position), count) : count;
            var read = _inner.Read(buffer, offset, n);

            if (read > 0)
            {
                _position += read;
            }
            else if (read == 0)
            {
                _endOfStream = true;
            }

            return read;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count,
            CancellationToken cancellationToken)
        {
            // if we are at the end of the stream, we cannot read anymore data
            if (_to != -1 && _from + _position >= _to)
            {
                _endOfStream = true;
                return 0;
            }

            // make sure that the source stream is in the expected position
            if (_inner.Position != _from + _position)
            {
                _inner.Seek(_from + _position, SeekOrigin.Begin);
            }

            var n = _to != -1 ? (int) Math.Min(_to - (_from + _position), count) : count;
            var read = await _inner.ReadAsync(buffer, offset, n, cancellationToken).ConfigureAwait(false);

            if (read > 0)
            {
                _position += read;
            }
            else if (read == 0)
            {
                _endOfStream = true;
            }

            return read;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            // if we are at the end of the stream, we cannot write anymore data
            if (_to != -1 && _from + _position + count > _to)
            {
                _endOfStream = _from + _position >= _to;
                throw new IOException();
            }

            // make sure that the source stream is in the expected position
            if (_inner.Position != _from + _position)
            {
                _inner.Seek(_from + _position, SeekOrigin.Begin);
            }

            _inner.Write(buffer, offset, count);
            _position += count;

            if (_to != -1 && _from + _position >= _to)
            {
                _endOfStream = true;
            }
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            // if we are at the end of the stream, we cannot write anymore data
            if (_to != -1 && _from + _position + count > _to)
            {
                _endOfStream = _from + _position >= _to;
                throw new IOException();
            }

            // make sure that the source stream is in the expected position
            if (_inner.Position != _from + _position)
            {
                _inner.Seek(_from + _position, SeekOrigin.Begin);
            }

            await _inner.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            _position += count;

            if (_to != -1 && _from + _position >= _to)
            {
                _endOfStream = true;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long real;

            switch (origin)
            {
                case SeekOrigin.Begin:
                    real = _from + offset;
                    break;
                case SeekOrigin.Current:
                    real = _from + _position + offset;
                    break;
                case SeekOrigin.End:
                    if (offset >= 0 || _to == -1 && !_endOfStream)
                    {
                        // We don't know if the underlying stream can seek past the end or not...
                        if ((real = _inner.Seek(offset, origin)) == -1)
                        {
                            return -1;
                        }
                    }
                    else if (_to == -1)
                    {
                        // seeking backwards from eos (which happens to be our current position)
                        real = _from + _position + offset;
                    }
                    else
                    {
                        // seeking backwards from a known position
                        real = _to + offset;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), "Invalid SeekOrigin specified");
            }

            // sanity check the resultant offset
            if (real < _from)
            {
                throw new IOException("Cannot seek to a position before the beginning of the stream");
            }

            // short-cut if we are seeking to our current position
            if (real == _from + _position)
            {
                return _position;
            }

            if (_to != -1 && real > _to)
            {
                throw new IOException("Cannot seek beyond the end of the stream");
            }

            if ((real = _inner.Seek(real, SeekOrigin.Begin)) == -1)
            {
                return -1;
            }

            // reset eos if appropriate
            if (_to != -1 && real < _to || _endOfStream && real < _from + _position)
            {
                _endOfStream = false;
            }

            _position = real - _from;

            return _position;
        }

        public override void Flush()
        {
            _inner.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return _inner.FlushAsync(cancellationToken);
        }

        public override void SetLength(long value)
        {
            if (_to == -1 || _from + value > _to)
            {
                var end = _inner.Length;
                if (_from + value > end)
                {
                    _inner.SetLength(_from + value);
                }

                _to = _from + value;
            }
            else
            {
                _to = _from + value;
            }
        }
    }
}
