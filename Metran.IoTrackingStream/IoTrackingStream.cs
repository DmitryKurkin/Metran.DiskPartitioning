using System;
using System.IO;

namespace Metran.IO.Streams
{
    /// <summary>
    /// Wraps a stream and tracks the total number of bytes that were read from and written to the stream. Reports the numbers upon closing
    /// </summary>
    public class IoTrackingStream : Stream
    {
        private readonly Stream _baseStream;

        private readonly ITrackingInfoConsumer _trackingInfoConsumer;

        private long _totalBytesRead;

        private long _totalBytesWritten;

        public IoTrackingStream(Stream baseStream, ITrackingInfoConsumer trackingInfoConsumer)
        {
            if (baseStream == null) throw new ArgumentNullException(nameof(baseStream));
            if (trackingInfoConsumer == null) throw new ArgumentNullException(nameof(trackingInfoConsumer));

            _baseStream = baseStream;
            _trackingInfoConsumer = trackingInfoConsumer;
        }

        public override bool CanRead => _baseStream.CanRead;

        public override bool CanSeek => _baseStream.CanSeek;

        public override bool CanWrite => _baseStream.CanWrite;

        public override long Length => _baseStream.Length;

        public override long Position
        {
            get { return _baseStream.Position; }
            set { _baseStream.Position = value; }
        }

        public override void Close()
        {
            _baseStream.Close();

            // assign...
            _trackingInfoConsumer.AssignTrackingInfo(_totalBytesRead, _totalBytesWritten);

            base.Close();
        }

        public override void Flush()
        {
            _baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytesRead = _baseStream.Read(buffer, offset, count);

            // track
            _totalBytesRead += bytesRead;

            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _baseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _baseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _baseStream.Write(buffer, offset, count);

            // track
            _totalBytesWritten += count;
        }
    }
}