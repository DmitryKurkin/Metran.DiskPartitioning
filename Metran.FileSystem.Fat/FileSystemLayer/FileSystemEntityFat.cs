using Metran.FileSystem.Fat.ClusterChainStreamLayer;
using Metran.FileSystem.Fat.VFATLayer;
using System;

namespace Metran.FileSystem.Fat.FileSystemLayer
{
    /// <summary>
    /// Provides the base class for a FAT file system entity
    /// </summary>
    public abstract class FileSystemEntityFat : IFileSystemEntity
    {
        protected IDirectoryEntry UnderlyingEntry;

        protected DirectoryFat UnderlyingParentDirectory;

        protected IClusterChainStreamManager StreamManager;

        protected FileSystemEntityFat(
            IDirectoryEntry entry,
            DirectoryFat parentDirectory,
            IClusterChainStreamManager streamManager)
            : this(streamManager)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            UnderlyingEntry = entry;
            UnderlyingParentDirectory = parentDirectory;
        }

        #region IFileSystemEntity members

        public virtual string Name => UnderlyingEntry.Name;

        public virtual FileSystemEntityAttributes Attributes
        {
            get { return GetAttributes(); }
            set
            {
                SetAttributes(value);
                OnChanged();
            }
        }

        public virtual DateTime CreationDate => UnderlyingEntry.CreationDate;

        public virtual DateTime LastAccessDate => UnderlyingEntry.LastAccessDate;

        public virtual DateTime LastWriteDate => UnderlyingEntry.LastWriteDate;

        // only our parent can determine whether we exist
        public virtual bool Exists => UnderlyingParentDirectory.EntityExists(this);

        public virtual IDirectory ParentDirectory => UnderlyingParentDirectory;

        public virtual void Rename(string newName)
        {
            // rename ourself with the help of our parent
            UnderlyingParentDirectory.RenameEntity(this, newName);

            OnChanged();
        }

        public abstract void Delete();

        #endregion

        internal IDirectoryEntry Entry => UnderlyingEntry;

        internal event EventHandler Changed;

        internal virtual void FinishCreation()
        {
            // generate and assign the creation date

            var creationDate = DateTime.Now;

            UnderlyingEntry.CreationDate = creationDate;
            UnderlyingEntry.LastAccessDate = creationDate;
            UnderlyingEntry.LastWriteDate = creationDate;
        }

        protected virtual FileSystemEntityAttributes GetAttributes()
        {
            var atts = FileSystemEntityAttributes.Normal;

            if ((UnderlyingEntry.EntryAttributes & DirectoryEntryAttributes.Archive) == DirectoryEntryAttributes.Archive)
            {
                atts |= FileSystemEntityAttributes.Archive;
            }

            if ((UnderlyingEntry.EntryAttributes & DirectoryEntryAttributes.Hidden) == DirectoryEntryAttributes.Hidden)
            {
                atts |= FileSystemEntityAttributes.Hidden;
            }

            if ((UnderlyingEntry.EntryAttributes & DirectoryEntryAttributes.ReadOnly) ==
                DirectoryEntryAttributes.ReadOnly)
            {
                atts |= FileSystemEntityAttributes.ReadOnly;
            }

            if ((UnderlyingEntry.EntryAttributes & DirectoryEntryAttributes.System) == DirectoryEntryAttributes.System)
            {
                atts |= FileSystemEntityAttributes.System;
            }

            return atts;
        }

        protected virtual void SetAttributes(FileSystemEntityAttributes atts)
        {
            UnderlyingEntry.EntryAttributes = 0;

            if ((atts & FileSystemEntityAttributes.Archive) == FileSystemEntityAttributes.Archive)
            {
                UnderlyingEntry.EntryAttributes |= DirectoryEntryAttributes.Archive;
            }

            if ((atts & FileSystemEntityAttributes.Hidden) == FileSystemEntityAttributes.Hidden)
            {
                UnderlyingEntry.EntryAttributes |= DirectoryEntryAttributes.Hidden;
            }

            if ((atts & FileSystemEntityAttributes.ReadOnly) == FileSystemEntityAttributes.ReadOnly)
            {
                UnderlyingEntry.EntryAttributes |= DirectoryEntryAttributes.ReadOnly;
            }

            if ((atts & FileSystemEntityAttributes.System) == FileSystemEntityAttributes.System)
            {
                UnderlyingEntry.EntryAttributes |= DirectoryEntryAttributes.System;
            }
        }

        protected virtual void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }

        protected FileSystemEntityFat()
        {
        }

        protected FileSystemEntityFat(IClusterChainStreamManager streamManager)
        {
            if (streamManager == null) throw new ArgumentNullException(nameof(streamManager));

            StreamManager = streamManager;
        }
    }
}