using System;

namespace Metran.FileSystem
{
    /// <summary>
    /// Defines properties that are common to all files and directories
    /// </summary>
    public interface IFileSystemEntity
    {
        string Name { get; }

        FileSystemEntityAttributes Attributes { get; set; }

        DateTime CreationDate { get; }

        DateTime LastAccessDate { get; }

        DateTime LastWriteDate { get; }

        bool Exists { get; }

        IDirectory ParentDirectory { get; }

        void Rename(string newName);

        void Delete();
    }
}