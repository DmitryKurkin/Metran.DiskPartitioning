using System;
using System.IO;

namespace Metran.IO.Streams
{
    /// <summary>
    /// Adds a buffering layer to read and write operations on a block device
    /// </summary>
    /// <remarks>
    /// Uses one buffer for both types of operations (read and write).
    /// Drops or flushes the buffer contents and starts to use the next available data block as a client changes the current operation type.
    /// Pads missing bytes in the buffer with zero bytes to the full block size on a flush operation
    /// </remarks>
    public class InputOutputBufferedStream : Stream
    {
        /// <summary>
        /// Specifies the current state of a buffered stream
        /// </summary>
        private enum BufferedStreamState
        {
            Initial,
            Reading,
            Writing
        }

        private static void Validate(byte[] buffer, int offset, int count)
        {
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
        }

        private readonly IBlockDevice _blockDevice;

        private readonly IPipeBuffer _pipeBuffer;

        private readonly int _blocksMultiplier;

        private BufferedStreamState _currentState = BufferedStreamState.Initial;

        private bool _isClosed;

        public InputOutputBufferedStream(IBlockDevice blockDevice, IPipeBuffer pipeBuffer, int blocksMultiplier = 1)
        {
            if (blockDevice == null) throw new ArgumentNullException(nameof(blockDevice));
            if (pipeBuffer == null) throw new ArgumentNullException(nameof(pipeBuffer));
            if (blocksMultiplier < 1)
                throw new ArgumentOutOfRangeException(nameof(blocksMultiplier), "The multiplier must be greater than 1");

            _blockDevice = blockDevice;
            _pipeBuffer = pipeBuffer;
            _blocksMultiplier = blocksMultiplier;
        }

        public override bool CanRead => !_isClosed && _blockDevice.SupportsReading;

        public override bool CanSeek => !_isClosed && _blockDevice.SupportsPositioning;

        public override bool CanWrite => !_isClosed && _blockDevice.SupportsWriting;

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { Seek(value, SeekOrigin.Begin); }
        }

        public override void Close()
        {
            AssertNotClosed();

            // don't forget to flush the remains of the buffered data (if the client has been writing to the device)
            Flush();

            _blockDevice.Close();
            base.Close();

            _isClosed = true;
        }

        public override void Flush()
        {
            // there is only one reason to flush data: if the client is writing.
            // if he is reading then just drop it.
            // a strange flush though

            switch (_currentState)
            {
                case BufferedStreamState.Writing:
                    FlushBufferedData();
                    break;
                case BufferedStreamState.Reading:
                    DropBufferedData();
                    break;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            AssertNotClosed();
            AssertCanRead();
            Validate(buffer, offset, count);

            // handle a possible change in the current state
            ChangeStateIfNeeded(BufferedStreamState.Reading);

            // we are being asked to read "count" bytes from the device.
            // we can read random amounts of data only from the buffer.
            // if it has the required number of bytes, we just read them.
            // if it does not, we have to feed the buffer from the device.
            // the problem here is that data might not be available
            // for feeding any more (we have read all blocks from the device)

            // the desired number of bytes to read
            var bytesToRead = count;

            // do we have enough buffered data?
            if (!IsBufferedDataAvailable(count))
            {
                // no, we don't

                // now we have to feed the buffer from the device:
                // don't forget there might not be enough data in the device
                if (!FeedBuffer(count))
                {
                    // the feeding was not successful: the device did not have enough data
                    // to feed the buffer to the desired number of bytes.

                    // we cannot return the desired number of bytes.
                    // the only thing we can do now is to return what the buffer has
                    bytesToRead = _pipeBuffer.BytesAvailable;
                }
            }

            // we already decided what number of bytes we read now
            _pipeBuffer.Read(buffer, offset, bytesToRead);

            return bytesToRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            AssertNotClosed();
            AssertCanPosition();

            // the interface of the block device does not allow us to do anything except this type of seeking
            if (origin != SeekOrigin.Begin)
            {
                throw new NotSupportedException();
            }

            // either flush or drop whatever we have now and position the device without changing the current state
            Flush();

            return _blockDevice.Position(offset);
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            AssertNotClosed();
            AssertCanWrite();
            Validate(buffer, offset, count);

            // handle a possible change in the current state
            ChangeStateIfNeeded(BufferedStreamState.Writing);

            // buffer the arrived data
            _pipeBuffer.Feed(buffer, offset, count);

            // a temp buffer used to move data between the pipe buffer and the device
            var blockData = new byte[_blockDevice.BlockSize*_blocksMultiplier];

            // write an integer number of buffered blocks to the device
            while (IsBufferedDataAvailable(blockData.Length))
            {
                // read a single block from the buffered data
                _pipeBuffer.Read(blockData);

                // write it to the device
                _blockDevice.WriteBlock(blockData);
            }
        }

        private void AssertNotClosed()
        {
            if (_isClosed)
            {
                throw new ObjectDisposedException("blockDevice", "The block device has been closed");
            }
        }

        private void AssertCanPosition()
        {
            if (!_blockDevice.SupportsPositioning)
            {
                throw new InvalidOperationException("The device does not support positioning");
            }
        }

        private void AssertCanRead()
        {
            if (!_blockDevice.SupportsReading)
            {
                throw new InvalidOperationException("The device does not support reading");
            }
        }

        private void AssertCanWrite()
        {
            if (!_blockDevice.SupportsWriting)
            {
                throw new InvalidOperationException("The device does not support writing");
            }
        }

        private void FlushBufferedData()
        {
            // do we have any data to flush?
            if (_pipeBuffer.BytesAvailable > 0)
            {
                // yes, we do.
                // now calculate the number of blocks that covers the data in the buffer

                var bytesToFlush = _blockDevice.BlockSize;
                while (_pipeBuffer.BytesAvailable > bytesToFlush)
                {
                    bytesToFlush += _blockDevice.BlockSize;
                }

                // allocate N blocks of zero bytes...
                var blockData = new byte[bytesToFlush];

                // ...and move there the REMAINS of the buffered data (the effect is
                // we have padded the remains of the buffered data with zeroes)
                _pipeBuffer.Read(blockData, 0, _pipeBuffer.BytesAvailable);

                // now flush it
                _blockDevice.WriteBlock(blockData);
            }
        }

        private void DropBufferedData()
        {
            // simply...
            _pipeBuffer.Reset();
        }

        private void ChangeStateIfNeeded(BufferedStreamState newState)
        {
            // is this really going to be a change of the current state?
            if (_currentState != newState)
            {
                // do what is needs to do with the current buffered data (either flush or drop)
                Flush();

                // now change the current state
                _currentState = newState;
            }
        }

        private bool IsBufferedDataAvailable(int requiredBytesCount)
        {
            return _pipeBuffer.BytesAvailable >= requiredBytesCount;
        }

        private bool FeedBuffer(int requiredBytesCount)
        {
            // this will be true if the buffer has the required number of bytes after the feeding
            bool feedingSuccessful;

            // feed the buffer until the data gets available or the device returns zero bytes
            int bytesFed;
            do
            {
                // read N blocks from the device
                var blockData = _blockDevice.ReadBlock(_blocksMultiplier);

                // feed them to the buffer
                bytesFed = _pipeBuffer.Feed(blockData);

                // is data available after this iteration?
                feedingSuccessful = IsBufferedDataAvailable(requiredBytesCount);
            } while (!feedingSuccessful && bytesFed != 0);

            return feedingSuccessful;
        }
    }
}