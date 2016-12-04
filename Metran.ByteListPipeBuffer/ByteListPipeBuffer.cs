using System;
using System.Collections.Generic;

namespace Metran.IO.Streams
{
    /// <summary>
    /// Implements a pipe buffer over a list of bytes
    /// </summary>
    public class ByteListPipeBuffer : IPipeBuffer
    {
        private const long HeadLimit = 10 * 1024 * 1024;

        private readonly List<byte> _internalBuffer = new List<byte>();

        private long _totalBytesRead;

        private long _totalBytesFed;

        private int _headIndex;

        int IPipeBuffer.BytesAvailable => _internalBuffer.Count - _headIndex;

        long IPipeBuffer.TotalBytesRead => _totalBytesRead;

        long IPipeBuffer.TotalBytesFed => _totalBytesFed;

        void IPipeBuffer.Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            if (offset + count > buffer.Length)
            {
                throw new ArgumentException(
                    $"The sum of offset and count ({offset + count}) is larger than the buffer length ({buffer.Length})");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), $"The offset is negative ({offset})");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), $"The count is negative ({count})");
            }

            var bytesAvailable = (this as IPipeBuffer).BytesAvailable;
            if (count > bytesAvailable)
            {
                throw new ArgumentException(
                    $"There are not enough bytes in the buffer. The requested count is {count}. The available count is {bytesAvailable}",
                    nameof(count));
            }

            // copy the requested number of bytes to the dest buffer
            _internalBuffer.CopyTo(_headIndex, buffer, offset, count);

            // advance the reading counter (and the number of bytes to discard) by "count" bytes
            _totalBytesRead += count;
            _headIndex += count;

            DiscardIfNeeded();
        }

        void IPipeBuffer.Read(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            (this as IPipeBuffer).Read(buffer, 0, buffer.Length);
        }

        int IPipeBuffer.Feed(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            if (offset + count > buffer.Length)
            {
                throw new ArgumentException(
                    $"The sum of offset and count ({offset + count}) is larger than the buffer length ({buffer.Length})");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), $"The offset is negative ({offset})");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), $"The count is negative ({count})");
            }

            // make a subset from the source buffer to add it to the internal buffer
            var bufferSubset = new byte[count];
            Array.Copy(buffer, offset, bufferSubset, 0, count);

            // add the subset to the internal buffer
            _internalBuffer.AddRange(bufferSubset);

            // advance the writing counter by "count" bytes
            _totalBytesFed += count;

            return count;
        }

        int IPipeBuffer.Feed(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            return (this as IPipeBuffer).Feed(buffer, 0, buffer.Length);
        }

        void IPipeBuffer.Reset()
        {
            _internalBuffer.Clear();
            _headIndex = 0;
        }

        private void DiscardIfNeeded()
        {
            if (_headIndex < HeadLimit) return;

            _internalBuffer.RemoveRange(0, _headIndex);
            _headIndex = 0;
        }
    }
}