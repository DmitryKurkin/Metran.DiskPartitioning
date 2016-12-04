using System.IO;

namespace Metran.FileSystem
{
    /// <summary>
    /// Defines properties that are specific to files
    /// </summary>
    public interface IFile : IFileSystemEntity
    {
        bool IsOpened { get; }

        long Length { get; }

        Stream OpenRead();

        Stream OpenWrite();
    }
}