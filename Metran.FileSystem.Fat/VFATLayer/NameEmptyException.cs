using System;

namespace Metran.FileSystem.Fat.VFATLayer
{
    /// <summary>
    /// The exception that is thrown when a name becomes empty after stripping ignored characters
    /// </summary>
    public class NameEmptyException : Exception
    {
        public NameEmptyException(string message)
            : base(message)
        {
        }
    }
}