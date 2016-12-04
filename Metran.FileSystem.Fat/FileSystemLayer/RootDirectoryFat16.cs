using Metran.FileSystem.Fat.ClusterChainStreamLayer;
using Metran.FileSystem.Fat.VFATLayer;
using System;
using System.IO;

namespace Metran.FileSystem.Fat.FileSystemLayer
{
    /// <summary>
    /// Represents a root directory on a FAT12 or FAT16 volume. Changes the behaviour of the RootDirectoryFat32 due to the fact the root directory on a FAT12/16 volume is sector-based. Considers the size of a sector to be 512 bytes
    /// </summary>
    /// <remarks>
    /// There is no reason to derive the class from the RootDirectoryFat32.
    /// Actually, this was done to not re-implement the base class'es methods.
    /// Truly, this must be a separate and independent implementation
    /// </remarks>
    public class RootDirectoryFat16 : RootDirectoryFat32
    {
        private const int BytesPerSector = 512;

        private readonly Stream _targetStream;

        private readonly long _startSector;

        private readonly long _sectorsCount;

        internal RootDirectoryFat16(
            Stream targetStream,
            long startSector,
            long sectorsCount,
            IClusterChainStreamManager streamManager,
            IDirectoryEntryManager entryManager)
            : base(streamManager, entryManager)
        {
            if (targetStream == null) throw new ArgumentNullException(nameof(targetStream));

            if (!targetStream.CanRead)
            {
                throw new ArgumentException("The target stream must be readable", nameof(targetStream));
            }

            if (!targetStream.CanSeek)
            {
                throw new ArgumentException("The target stream must be seekable", nameof(targetStream));
            }

            if (!targetStream.CanWrite)
            {
                throw new ArgumentException("The target stream must be writeable", nameof(targetStream));
            }

            if (startSector < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startSector),
                    $"The start sector is negative: {startSector}");
            }

            if (sectorsCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sectorsCount),
                    $"The sectors count is negative: {sectorsCount}");
            }

            _targetStream = targetStream;
            _startSector = startSector;
            _sectorsCount = sectorsCount;
        }

        internal override void FinishCreation()
        {
            // we just need to reset the data of our sectors

            // go to the first sector of the dir
            PositionStream();

            // zeroes
            var sectorData = new byte[BytesPerSector];

            // reset the sectors
            for (var i = 0; i < _sectorsCount; i++)
            {
                _targetStream.Write(sectorData, 0, sectorData.Length);
            }
        }

        internal override void LoadRecursive()
        {
            // go to the first sector of the dir
            PositionStream();

            // the only difference from the base implementation is we should use the target stream
            var entries = EntryManager.LoadEntries(_targetStream, false);

            for (var i = 0; i < entries.Length; i++)
            {
                // we are interested in 3 cases: (actually, the check might be more intelligent)))
                // 1. a directory
                // 2. a volume label
                // 3. a file (implied implicitly after the first two)

                if ((entries[i].EntryAttributes & DirectoryEntryAttributes.Directory) ==
                    DirectoryEntryAttributes.Directory)
                {
                    // this is a directory
                    var directory = new DirectoryFat(entries[i], this, StreamManager, EntryManager);

                    // add it to our list (without raising events)
                    AddFileSystemEntity(directory, true);

                    // continue the process of recursive loading in the subdir we have just created
                    directory.LoadRecursive();
                }
                else if ((entries[i].EntryAttributes & DirectoryEntryAttributes.VolumeLabel) ==
                         DirectoryEntryAttributes.VolumeLabel)
                {
                    // this is a volume label

                    // did we already assign the label?
                    if (VolumeLabelEntry != null)
                    {
                        throw new FileSystemCorruptedException("Several volume labels found");
                    }

                    // assign it
                    VolumeLabelEntry = entries[i];
                }
                else
                {
                    // this should be a file
                    var file = new FileFat(entries[i], this, StreamManager);

                    // add it to our list (without raising events)
                    AddFileSystemEntity(file, true);
                }
            }
        }

        internal override int FirstCluster
        {
            get
            {
                // we'd better not derive from the RootDirectoryFat32
                throw new NotSupportedException();
            }
        }

        private void PositionStream()
        {
            _targetStream.Position = _startSector * BytesPerSector;
        }

        protected override void ValidateHasFreeRoomFor(IDirectoryEntry entry)
        {
            var entrySize = EntryManager.GetSize(entry);

            if (TotalEntriesSize + entrySize > _sectorsCount * BytesPerSector)
            {
                throw new MaxDirectorySizeReachedException(
                    "There is no free room for the entry in the directory: the maximum allowable directory size has been reached");
            }
        }

        protected override void FlushEntries()
        {
            // the stream always exists, we don't need neither to open nor to close it

            // go to the start sector
            PositionStream();

            // flush the label if exists
            VolumeLabelEntry?.Save(_targetStream);

            // flush the entries
            foreach (var e in (this as IDirectoryEntryContainer).Entries)
            {
                e.Save(_targetStream);
            }
        }
    }
}