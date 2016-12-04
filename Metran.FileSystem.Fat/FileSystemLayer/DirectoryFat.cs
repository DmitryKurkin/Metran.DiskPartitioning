using Metran.FileSystem.Fat.ClusterChainStreamLayer;
using Metran.FileSystem.Fat.VFATLayer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Metran.FileSystem.Fat.FileSystemLayer
{
    /// <summary>
    /// Implements IDirectory the way specific to the FAT file systems family
    /// </summary>
    public class DirectoryFat : FileSystemEntityFat, IDirectory, IDirectoryEntryContainer
    {
        protected const int MaxSize = 0xFFFF*32;

        protected IDirectoryEntry DotEntry;

        protected IDirectoryEntry DotdotEntry;

        protected List<FileSystemEntityFat> FileSystemEntities;

        protected IDirectoryEntryManager EntryManager;

        protected bool IsDeletingRecursively;

        internal DirectoryFat(
            IDirectoryEntry entry,
            DirectoryFat parentDirectory,
            IClusterChainStreamManager streamManager,
            IDirectoryEntryManager entryManager)
            : base(entry, parentDirectory, streamManager)
        {
            if (parentDirectory == null) throw new ArgumentNullException(nameof(parentDirectory));
            if (entryManager == null) throw new ArgumentNullException(nameof(entryManager));

            FileSystemEntities = new List<FileSystemEntityFat>();

            EntryManager = entryManager;
        }

        #region IDirectory members

        public virtual bool IsRoot => false;

        public virtual IDirectory CreateSubdirectory(string name)
        {
            // an entry for the new subdir
            var dirEntry = EntryManager.CreateEntry(this, name);
            dirEntry.EntryAttributes = DirectoryEntryAttributes.Directory;

            // check the current dir size (we don't catch a possible exception here
            // because at this point our state has not changed yet and there is nothing to restore)
            ValidateHasFreeRoomFor(dirEntry);

            // the subdir itself
            var directory = new DirectoryFat(dirEntry, this, StreamManager, EntryManager);
            // finish the process (generate dates, allocate a cluster, etc)
            directory.FinishCreation();

            // add it to our list
            AddFileSystemEntity(directory, false);

            return directory;
        }

        public virtual IFile CreateFile(string name)
        {
            // an entry for the new file
            var fileEntry = EntryManager.CreateEntry(this, name);

            // check the current dir size (we don't catch a possible exception here
            // because at this point our state has not changed yet and there is nothing to restore)
            ValidateHasFreeRoomFor(fileEntry);

            // the file itself
            var file = new FileFat(fileEntry, this, StreamManager);
            // finish the process (generate dates, allocate a cluster, etc)
            file.FinishCreation();

            // add it to our list
            AddFileSystemEntity(file, false);

            return file;
        }

        public virtual IFileSystemEntity[] GetFileSystemEntities()
        {
            // it's simple...
            // ReSharper disable once CoVariantArrayConversion
            return FileSystemEntities.ToArray();
        }

        public virtual IDirectory[] GetDirectories()
        {
            var directories = new List<IDirectory>();

            // just collect the proper types...
            foreach (var fse in FileSystemEntities)
            {
                var dir = fse as IDirectory;

                if (dir != null)
                {
                    directories.Add(dir);
                }
            }

            return directories.ToArray();
        }

        public virtual IFile[] GetFiles()
        {
            var files = new List<IFile>();

            // just collect the proper types...
            foreach (var fse in FileSystemEntities)
            {
                var file = fse as IFile;

                if (file != null)
                {
                    files.Add(file);
                }
            }

            return files.ToArray();
        }

        public virtual void DeleteRecursive()
        {
            // set the flag to not raise events
            IsDeletingRecursively = true;

            // delete every file
            foreach (var f in GetFiles())
            {
                f.Delete();
            }

            // start the process of recursive deletion for every subdir
            foreach (var d in GetDirectories())
            {
                d.DeleteRecursive();
            }

            // all entities have now gone, finally, delete our stream
            Delete();
        }

        #endregion

        #region FileSystemEntityFat32 members

        public override void Delete()
        {
            // this is not a recursive deletion
            if (FileSystemEntities.Count > 0)
            {
                throw new InvalidOperationException("The directory is not empty");
            }

            // delete our stream (at least one cluster was allocated for the dot and dotdot entries in FinishCreation)
            StreamManager.DeleteStream(UnderlyingEntry.FirstCluster);

            // ask the parent to remove us from its list of entities
            UnderlyingParentDirectory.RemoveEntity(this);
        }

        #endregion

        internal override void FinishCreation()
        {
            // the base implementation assigns the creation date
            base.FinishCreation();

            // this is a first creation, we need to create a new stream (according to the spec, reset the cluster)
            int firstCluster;
            using (var output = StreamManager.CreateStream(out firstCluster, true))
            {
                // save the first cluster in the entry
                UnderlyingEntry.FirstCluster = firstCluster;

                // create always existing dot and dotdot entries
                DotEntry = EntryManager.CreateDotEntry();
                DotdotEntry = EntryManager.CreateDotdotEntry();

                // assign them dates according to the spec:
                // creation
                DotEntry.CreationDate = UnderlyingEntry.CreationDate;
                DotdotEntry.CreationDate = UnderlyingEntry.CreationDate;
                // last access
                DotEntry.LastAccessDate = UnderlyingEntry.LastAccessDate;
                DotdotEntry.LastAccessDate = UnderlyingEntry.LastAccessDate;
                // last write
                DotEntry.LastWriteDate = UnderlyingEntry.LastWriteDate;
                DotdotEntry.LastWriteDate = UnderlyingEntry.LastWriteDate;

                // assign them cluster numbers according to the spec:
                // points to us
                DotEntry.FirstCluster = UnderlyingEntry.FirstCluster;
                // points to the parent
                DotdotEntry.FirstCluster = UnderlyingParentDirectory.Entry.FirstCluster;

                // write them to the disk (we could call the flushEntries though)
                DotEntry.Save(output);
                DotdotEntry.Save(output);
            }
        }

        internal virtual bool EntityExists(FileSystemEntityFat entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var result = false;

            // to exist, it must be contained within us...
            if (Contains(entity))
            {
                // ..and we must exist too
                result = UnderlyingParentDirectory.EntityExists(this);
            }

            return result;
        }

        internal virtual void RenameEntity(FileSystemEntityFat entity, string newName)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            if (!Contains(entity))
            {
                throw new ArgumentException("The entity is not contained in the directory", nameof(entity));
            }

            if (string.IsNullOrEmpty(newName))
            {
                throw new ArgumentException("The name must not be empty", nameof(newName));
            }

            // save it for now (to be able to restore it later in case of an error)
            var oldName = entity.Name;

            // use the manager for this task
            EntryManager.RenameEntry(entity.Entry, this, newName);

            try
            {
                // the size of the entry might have changed. we have to check ourself again
                ValidateHasFreeRoomFor(entity.Entry);
            }
            catch (MaxDirectorySizeReachedException)
            {
                // we don't want to live with the new name, return the old one
                EntryManager.RenameEntry(entity.Entry, this, oldName);

                // give to know the client
                throw;
            }
        }

        internal virtual void RemoveEntity(FileSystemEntityFat entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            if (!Contains(entity))
            {
                throw new ArgumentException("The entity is not contained in the directory", nameof(entity));
            }

            // everything is in the method
            RemoveFileSystemEntity(entity);
        }

        internal virtual void LoadRecursive()
        {
            // use the entry manager to get this list of entries from our stream
            // (this considers the first cluster is already assigned!)
            IDirectoryEntry[] entries;

            using (var input = StreamManager.OpenStreamForReading(UnderlyingEntry.FirstCluster))
            {
                entries = EntryManager.LoadEntries(input, false);
            }

            // there must be at least dot and dotdot entries...
            if (entries.Length < 2)
            {
                throw new FileSystemCorruptedException("The required directory entries were not found");
            }

            // actually, we have to define a special method in the layer below for this check
            // (whether these two are dot and dotdot)!
            DotEntry = entries[0];
            DotdotEntry = entries[1];

            // NOTE: we consider the list of entities is EMPTY at this point, so we don't clear it!
            // start from entry number 2 (the first two were processed)
            for (int i = 2; i < entries.Length; i++)
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
                    throw new FileSystemCorruptedException("An unexpected volume label found");
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

        protected virtual int TotalEntriesSize
        {
            get
            {
                // start from the dot and dotdot...
                var totalEntriesSize = EntryManager.GetSize(DotEntry) + EntryManager.GetSize(DotdotEntry);

                // ...and add the others
                foreach (var e in (this as IDirectoryEntryContainer).Entries)
                {
                    totalEntriesSize += EntryManager.GetSize(e);
                }

                return totalEntriesSize;
            }
        }

        protected virtual void ValidateHasFreeRoomFor(IDirectoryEntry entryToValidate)
        {
            var entrySize = EntryManager.GetSize(entryToValidate);

            if (TotalEntriesSize + entrySize > MaxSize)
            {
                throw new MaxDirectorySizeReachedException(
                    "There is no free room for the entry in the directory: the maximum allowable directory size has been reached");
            }
        }

        protected virtual void FlushEntries()
        {
            // according to the spec, reset the clusters
            using (var output = StreamManager.OpenStreamForWriting(UnderlyingEntry.FirstCluster, true))
            {
                // the dot and dotdot entries go first
                DotEntry.Save(output);
                DotdotEntry.Save(output);

                // the contents go next
                foreach (var e in (this as IDirectoryEntryContainer).Entries)
                {
                    e.Save(output);
                }
            }
        }

        protected virtual void FileSystemEntityChanged(object sender, EventArgs e)
        {
            // one of our entities has changed its properties, we need to update our data on the disk
            FlushEntries();
        }

        protected virtual void AddFileSystemEntity(FileSystemEntityFat fse, bool isLoadingRecursively)
        {
            // listen to when it changes
            fse.Changed += FileSystemEntityChanged;

            // make it one of our entities
            FileSystemEntities.Add(fse);

            // if this is not a recursive loading process
            if (!isLoadingRecursively)
            {
                // update the list of entries on the disk (with the new one)
                FlushEntries();

                // we have changed, give it to know to our parent
                var lastAccessDate = DateTime.Now;
                UnderlyingEntry.LastAccessDate = lastAccessDate;
                UnderlyingEntry.LastWriteDate = lastAccessDate;
                OnChanged();
            }
        }

        protected virtual void RemoveFileSystemEntity(FileSystemEntityFat fse)
        {
            // don't listen to it anymore
            fse.Changed -= FileSystemEntityChanged;

            // this is not our entity anymore
            FileSystemEntities.Remove(fse);

            // if we are not deleting ourself recursively
            if (!IsDeletingRecursively)
            {
                // update the list of entries on the disk (without the one being removed)
                FlushEntries();

                // we have changed, give it to know to our parent
                var lastAccessDate = DateTime.Now;
                UnderlyingEntry.LastAccessDate = lastAccessDate;
                UnderlyingEntry.LastWriteDate = lastAccessDate;
                OnChanged();
            }
        }

        protected virtual bool Contains(FileSystemEntityFat entity)
        {
            // it's simple...
            return FileSystemEntities.Contains(entity);
        }

        ReadOnlyCollection<IDirectoryEntry> IDirectoryEntryContainer.Entries
        {
            get
            {
                var entries = new List<IDirectoryEntry>();

                // just collect...
                foreach (var fse in FileSystemEntities)
                {
                    entries.Add(fse.Entry);
                }

                return entries.AsReadOnly();
            }
        }

        protected DirectoryFat(IDirectoryEntryManager entryManager)
        {
            if (entryManager == null) throw new ArgumentNullException(nameof(entryManager));

            FileSystemEntities = new List<FileSystemEntityFat>();

            EntryManager = entryManager;
        }

        protected DirectoryFat(IClusterChainStreamManager streamManager, IDirectoryEntryManager entryManager)
            : base(streamManager)
        {
            if (entryManager == null) throw new ArgumentNullException(nameof(entryManager));

            FileSystemEntities = new List<FileSystemEntityFat>();

            EntryManager = entryManager;
        }
    }
}