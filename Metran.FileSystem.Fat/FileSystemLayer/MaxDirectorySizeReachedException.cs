using System;

namespace Metran.FileSystem.Fat.FileSystemLayer
{
    /// <summary>
    /// The exception that is thrown when a directory reaches the maximum allowable size
    /// </summary>
    public class MaxDirectorySizeReachedException : Exception
    {
        public MaxDirectorySizeReachedException(string message)
            : base(message)
        {
        }
    }
}