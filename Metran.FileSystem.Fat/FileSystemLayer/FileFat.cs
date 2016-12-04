using Metran.FileSystem.Fat.ClusterChainStreamLayer;
using Metran.FileSystem.Fat.VFATLayer;
using Metran.IO.Streams;
using System;
using System.IO;

namespace Metran.FileSystem.Fat.FileSystemLayer
{
    /// <summary>
    /// Implements IFile the way specific to the FAT file systems family
    /// </summary>
    /// <remarks>The implementation depends heavy on the DirectoryFat class</remarks>
    public class FileFat : FileSystemEntityFat, IFile, ITrackingInfoConsumer
    {
        private const long MaxFileSize = 0xFFFFFFFF;

        private bool _isBeingRead;

        private bool _isBeingWritten;

        internal FileFat(
            IDirectoryEntry entry,
            DirectoryFat parentDirectory,
            IClusterChainStreamManager streamManager)
            : base(entry, parentDirectory, streamManager)
        {
            if (parentDirectory == null) throw new ArgumentNullException(nameof(parentDirectory));
        }

        #region IFile members

        public bool IsOpened => _isBeingRead || _isBeingWritten;

        public long Length => UnderlyingEntry.Size;

        public Stream OpenRead()
        {
            // even for read operations we support just one client at a time
            AssertNotOpened();

            // if we don't have any associated data, there is no reason to read from us
            AssertAllocated();

            // a simple cluster chain stream
            var chainStream = StreamManager.OpenStreamForReading(UnderlyingEntry.FirstCluster);

            // limit it to the size of the file
            var constrainedStream = new ConstrainedReadingStream(chainStream, UnderlyingEntry.Size);

            // tell us when the client closes the stream (to update the access date)
            var trackingStream = new IoTrackingStream(constrainedStream, this);

            _isBeingRead = true;

            return trackingStream;
        }

        public Stream OpenWrite()
        {
            // if somebody is reading from or writing to us,
            // we cannot allow a new client to write to us because this will change the size
            AssertNotOpened();

            // a simple cluster chain stream
            Stream chainStream;

            // is this the first write?
            if (IsAllocated)
            {
                // data already exists, open the stream (don't reset clusters data: read operations are limited by the file size)
                chainStream = StreamManager.OpenStreamForWriting(UnderlyingEntry.FirstCluster, false);
            }
            else
            {
                // no data is allocated, create a new stream (don't reset clusters data: read operations are limited by the file size)
                int firstCluster;
                chainStream = StreamManager.CreateStream(out firstCluster, false);

                // save the first cluster in the entry
                UnderlyingEntry.FirstCluster = firstCluster;
            }

            // limit it to the maximum file size
            var constrainedStream = new ConstrainedWritingStream(chainStream, MaxFileSize);

            // tell us when the client closes the stream (to update the dates and the size)
            var trackingStream = new IoTrackingStream(constrainedStream, this);

            _isBeingWritten = true;

            return trackingStream;
        }

        #endregion

        #region FileSystemEntityFat32 members

        public override void Delete()
        {
            // if somebody is reading from or writing to us, we cannot delete our stream
            AssertNotOpened();

            // we don't need to delete our stream if we didn't allocate any single cluster
            if (IsAllocated)
            {
                StreamManager.DeleteStream(UnderlyingEntry.FirstCluster);
            }

            // ask the parent to remove us from its list of entities
            UnderlyingParentDirectory.RemoveEntity(this);
        }

        #endregion

        void ITrackingInfoConsumer.AssignTrackingInfo(long totalBytesRead, long totalBytesWritten)
        {
            if (_isBeingRead)
            {
                // we are being read

                // this is an act of access
                UnderlyingEntry.LastAccessDate = DateTime.Now;

                _isBeingRead = false;

                // notify the parent we have changed
                OnChanged();
            }
            else if (_isBeingWritten)
            {
                // we are being written

                // the size should have changed
                UnderlyingEntry.Size = (uint)totalBytesWritten;

                // did the client set the size to zero?
                if (totalBytesWritten == 0)
                {
                    // ok, we don't want to spend a single cluster for nothing
                    StreamManager.DeleteStream(UnderlyingEntry.FirstCluster);
                    UnderlyingEntry.FirstCluster = Utils.ClusterNotAllocated;
                }

                // this is an act of write access
                var lastAccessDate = DateTime.Now;
                UnderlyingEntry.LastAccessDate = lastAccessDate;
                UnderlyingEntry.LastWriteDate = lastAccessDate;

                _isBeingWritten = false;

                // notify the parent we have changed
                OnChanged();
            }
        }

        private bool IsAllocated => UnderlyingEntry.FirstCluster != Utils.ClusterNotAllocated;

        private void AssertNotOpened()
        {
            if (IsOpened)
            {
                throw new InvalidOperationException(
                    "The file is currently being used. Concurrent operations are not supported");
            }
        }

        private void AssertAllocated()
        {
            if (!IsAllocated)
            {
                throw new InvalidOperationException("The file has not been allocated yet");
            }
        }
    }
}