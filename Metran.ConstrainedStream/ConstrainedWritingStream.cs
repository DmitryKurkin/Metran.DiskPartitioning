using System;
using System.IO;

namespace Metran.IO.Streams
{
    /// <summary>
    /// Wraps a stream and limits its write operations to an expected length.
    /// Throws an EndOfStreamException if the expected length has been reached. Rejects all positioning and length changing requests
    /// </summary>
    public class ConstrainedWritingStream : Stream
    {
        private readonly Stream _baseStream;

        private readonly long _expectedLength;

        private long _totalBytesWritten;

        public ConstrainedWritingStream(Stream baseStream, long expectedLength)
        {
            if (baseStream == null) throw new ArgumentNullException(nameof(baseStream));

            if (expectedLength < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(expectedLength),
                    $"The expected length is negative: {expectedLength}");
            }

            _baseStream = baseStream;
            _expectedLength = expectedLength;
        }

        public override bool CanRead => _baseStream.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => _baseStream.CanWrite;

        public override long Length => _baseStream.Length;

        public override long Position
        {
            get { return _baseStream.Position; }
            set { throw new NotSupportedException(); }
        }

        public override void Close()
        {
            _baseStream.Close();

            base.Close();
        }

        public override void Flush()
        {
            _baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _baseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (BytesAvailableForWriting == 0)
            {
                throw new EndOfStreamException("The maximum allowable number of bytes has been written");
            }

            // ensure we are not going to cross the defined border
            EnsureExpectedLength(ref count);

            _baseStream.Write(buffer, offset, count);

            _totalBytesWritten += count;
        }

        private long BytesAvailableForWriting
        {
            get
            {
                // the number of bytes that can still be written according to the expected length
                var bytesAvailableForWriting = _expectedLength - _totalBytesWritten;

                return bytesAvailableForWriting;
            }
        }

        private void EnsureExpectedLength(ref int requiredCount)
        {
            // if the required number is larger than the available number, we truncate it
            if (requiredCount > BytesAvailableForWriting)
            {
                requiredCount = (int)BytesAvailableForWriting;
            }
        }
    }
}