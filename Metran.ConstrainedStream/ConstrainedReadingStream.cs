using System;
using System.IO;

namespace Metran.IO.Streams
{
    /// <summary>
    /// Wraps a stream and limits its read operations to an expected length.
    /// Returns zero bytes if the expected length has been reached. Rejects all positioning and length changing requests
    /// </summary>
    public class ConstrainedReadingStream : Stream
    {
        private readonly Stream _baseStream;

        private readonly long _expectedLength;

        private long _totalBytesRead;

        public ConstrainedReadingStream(Stream baseStream, long expectedLength)
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
            // ensure we are not going to cross the defined border
            EnsureExpectedLength(ref count);

            var bytesRead = _baseStream.Read(buffer, offset, count);

            _totalBytesRead += bytesRead;

            return bytesRead;
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
            _baseStream.Write(buffer, offset, count);
        }

        private long BytesAvailableForReading
        {
            get
            {
                // the number of bytes that can still be read according to the expected length
                var bytesAvailableForReading = _expectedLength - _totalBytesRead;

                return bytesAvailableForReading;
            }
        }

        private void EnsureExpectedLength(ref int requiredCount)
        {
            // if the required number is larger than the available number, we truncate it
            if (requiredCount > BytesAvailableForReading)
            {
                requiredCount = (int)BytesAvailableForReading;
            }
        }
    }
}