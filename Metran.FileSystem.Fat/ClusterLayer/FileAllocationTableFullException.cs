using System;

namespace Metran.FileSystem.Fat.ClusterLayer
{
    /// <summary>
    /// The exception that is thrown when a FAT is full
    /// </summary>
    public class FileAllocationTableFullException : Exception
    {
        public FileAllocationTableFullException(string message)
            : base(message)
        {
        }
    }
}