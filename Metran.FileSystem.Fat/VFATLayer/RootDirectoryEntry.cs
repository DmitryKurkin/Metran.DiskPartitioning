using System;
using System.IO;

namespace Metran.FileSystem.Fat.VFATLayer
{
    /// <summary>
    /// Represents the directory entry of the root directory on a FAT32 volume. The rest of the properties are not supported
    /// </summary>
    public class RootDirectoryEntry : IDirectoryEntry
    {
        private const string RootDirectoryName = "";
        private const DirectoryEntryAttributes RootDirectoryAttributes = DirectoryEntryAttributes.Directory;
        private const int RootDirectoryFirstCluster = 0;

        string IDirectoryEntry.Name => RootDirectoryName;

        DirectoryEntryAttributes IDirectoryEntry.EntryAttributes
        {
            get { return RootDirectoryAttributes; }
            set { throw new NotSupportedException(); }
        }

        DateTime IDirectoryEntry.CreationDate
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        DateTime IDirectoryEntry.LastAccessDate
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        DateTime IDirectoryEntry.LastWriteDate
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        int IDirectoryEntry.FirstCluster
        {
            get { return RootDirectoryFirstCluster; }
            set { throw new NotSupportedException(); }
        }

        uint IDirectoryEntry.Size
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        void IDirectoryEntry.Save(Stream output)
        {
            throw new NotSupportedException();
        }
    }
}