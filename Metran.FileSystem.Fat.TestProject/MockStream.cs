using System;
using System.IO;

namespace Metran.FileSystem.Fat.TestProject
{
    internal class MockStream : Stream
    {
        private bool isClosed;

        private bool canRead;
        private bool canSeek;
        private bool canWrite;

        internal MockStream( bool canRead, bool canSeek, bool canWrite )
        {
            this.canRead = canRead;
            this.canSeek = canSeek;
            this.canWrite = canWrite;
        }
        internal MockStream( bool isClosed )
            : this( true, true, true )
        {
            this.isClosed = isClosed;
        }

        public override bool CanRead
        {
            get
            {
                return canRead;
            }
        }
        public override bool CanSeek
        {
            get
            {
                return canSeek;
            }
        }
        public override bool CanWrite
        {
            get
            {
                return canWrite;
            }
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }
        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override int Read( byte[] buffer, int offset, int count )
        {
            throw new NotSupportedException();
        }
        public override long Seek( long offset, SeekOrigin origin )
        {
            throw new NotSupportedException();
        }
        public override void SetLength( long value )
        {
            throw new NotSupportedException();
        }
        public override void Write( byte[] buffer, int offset, int count )
        {
            throw new NotSupportedException();
        }
    }
}