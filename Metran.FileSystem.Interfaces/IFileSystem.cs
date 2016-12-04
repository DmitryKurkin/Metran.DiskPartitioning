using System;

namespace Metran.FileSystem
{
    /// <summary>
    /// Represents a generic file system. Manages generic files and directories
    /// </summary>
    public interface IFileSystem : IDisposable
    {
        IDirectory RootDirectory { get; }

        void Load();

        void Format();

        void Flush();
    }
}