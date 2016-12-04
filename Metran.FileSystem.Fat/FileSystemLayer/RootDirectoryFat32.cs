using Metran.FileSystem.Fat.ClusterChainStreamLayer;
using Metran.FileSystem.Fat.VFATLayer;
using System;

namespace Metran.FileSystem.Fat.FileSystemLayer
{
    /// <summary>
    /// Represents a root directory on a FAT32 volume. Changes the behaviour of the DirectoryFat according to the specification
    /// </summary>
    public class RootDirectoryFat32 : DirectoryFat
    {
        protected int UnderlyingFirstCluster;

        protected IDirectoryEntry VolumeLabelEntry;

        internal RootDirectoryFat32(
            int firstCluster,
            IClusterChainStreamManager streamManager,
            IDirectoryEntryManager entryManager)
            : base(streamManager, entryManager)
        {
            UnderlyingFirstCluster = firstCluster;

            UnderlyingEntry = entryManager.CreateRootDirectoryEntry();
        }

        internal RootDirectoryFat32(IClusterChainStreamManager streamManager, IDirectoryEntryManager entryManager)
            : base(streamManager, entryManager)
        {
            UnderlyingEntry = entryManager.CreateRootDirectoryEntry();
        }

        #region IDirectory members

        public override bool IsRoot => true;

        public override void DeleteRecursive()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IFileSystemEntity members

        public override bool Exists => true;

        public override void Rename(string newName)
        {
            throw new NotSupportedException();
        }

        public override void Delete()
        {
            throw new NotSupportedException();
        }

        #endregion

        internal override void FinishCreation()
        {
            // the first base implementation creates the dot and dotdot entries,
            // the second base implementation writes the creation date and time.
            // we don't call them because we don't have neither the entries nor the dates/times

            // also, don't forget that the first cluster is contained separately from the entry:
            // in the entry it must always be zero (dotdot entries of our children must have zero at the FirstCluster property)

            // according to the spec, reset the cluster
            // ReSharper disable once UnusedVariable
            using (var output = StreamManager.CreateStream(out UnderlyingFirstCluster, true))
            {
                // nothing to do (there are no entries that require special handling)
            }
        }

        internal override bool EntityExists(FileSystemEntityFat entity)
        {
            // we don't have the parent directory, so just check whether the entity is contained here

            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var result = Contains(entity);

            return result;
        }

        internal override void LoadRecursive()
        {
            // just don't forget the dot and dotdot entries do not exist in the root directory

            // use the entry manager to get this list of entries from our stream
            // (this considers the first cluster is already assigned!)
            IDirectoryEntry[] entries;

            using (var input = StreamManager.OpenStreamForReading(UnderlyingFirstCluster))
            {
                entries = EntryManager.LoadEntries(input, false);
            }

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

        internal virtual int FirstCluster => UnderlyingFirstCluster;

        internal virtual bool VolumeLabelExists => VolumeLabelEntry != null;

        internal virtual string VolumeLabel
        {
            get
            {
                if (!VolumeLabelExists)
                {
                    throw new InvalidOperationException("The volume label does not exist");
                }

                return VolumeLabelEntry.Name;
            }
        }

        internal virtual void AssignVolumeLabel(string volumeLabel)
        {
            if (string.IsNullOrEmpty(volumeLabel))
            {
                throw new ArgumentException("The label name must not be empty", nameof(volumeLabel));
            }

            // create a new volume label
            var newVolumeLabelEntry = EntryManager.CreateVolumeLabelEntry();

            // check if there is free room for it
            ValidateHasFreeRoomFor(newVolumeLabelEntry);

            // assign it the name
            EntryManager.RenameVolumeLabelEntry(newVolumeLabelEntry, volumeLabel);

            // save it
            VolumeLabelEntry = newVolumeLabelEntry;

            // flush it to the disk along with the others
            FlushEntries();
        }

        internal virtual void DeleteVolumeLabel()
        {
            // just if it already exists...
            if (VolumeLabelExists)
            {
                // delete it
                VolumeLabelEntry = null;

                // flush the updated contents
                FlushEntries();
            }
        }

        protected override int TotalEntriesSize
        {
            get
            {
                // the dot and dotdot do not exist, start with zero...
                var totalEntriesSize = 0;

                // ...and add the others
                foreach (var e in (this as IDirectoryEntryContainer).Entries)
                {
                    totalEntriesSize += EntryManager.GetSize(e);
                }

                return totalEntriesSize;
            }
        }

        protected override void FlushEntries()
        {
            // 1. we don't have the dot and dotdot entries, so we don't flush them
            // 2. also, don't forget to flush a volume label entry if it exists
            // 3. (or even 1.) entry.FirstCluster is not used! firstCluster is used instead

            // according to the spec, reset the clusters
            using (var output = StreamManager.OpenStreamForWriting(UnderlyingFirstCluster, true))
            {
                VolumeLabelEntry?.Save(output);

                foreach (var e in (this as IDirectoryEntryContainer).Entries)
                {
                    e.Save(output);
                }
            }
        }

        protected override void AddFileSystemEntity(FileSystemEntityFat fse, bool isLoadingRecursively)
        {
            // we don't support the dates and times, so we don't write them

            fse.Changed += FileSystemEntityChanged;

            FileSystemEntities.Add(fse);

            if (!isLoadingRecursively)
            {
                FlushEntries();
            }
        }

        protected override void RemoveFileSystemEntity(FileSystemEntityFat fse)
        {
            // we don't support the dates and times, so we don't write them

            fse.Changed -= FileSystemEntityChanged;

            FileSystemEntities.Remove(fse);

            // we don't need to check isDeletingRecursively because we don't support that operation
            FlushEntries();
        }
    }
}