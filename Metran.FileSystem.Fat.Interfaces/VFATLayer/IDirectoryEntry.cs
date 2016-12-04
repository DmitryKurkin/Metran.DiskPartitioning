using System;
using System.IO;

namespace Metran.FileSystem.Fat.VFATLayer
{
    /// <summary>
    /// Represents a FAT directory entry
    /// </summary>
    public interface IDirectoryEntry
    {
        string Name { get; }

        DirectoryEntryAttributes EntryAttributes { get; set; }

        DateTime CreationDate { get; set; }

        DateTime LastAccessDate { get; set; }

        DateTime LastWriteDate { get; set; }

        int FirstCluster { get; set; }

        uint Size { get; set; }

        void Save(Stream output);
    }
}