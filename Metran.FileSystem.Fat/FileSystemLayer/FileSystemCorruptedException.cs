using System;

namespace Metran.FileSystem.Fat.FileSystemLayer
{
    /// <summary>
    /// The exception that is thrown when the format of a file system is damaged
    /// </summary>
    public class FileSystemCorruptedException : Exception
    {
        public FileSystemCorruptedException(string message)
            : base(message)
        {
        }
    }
}