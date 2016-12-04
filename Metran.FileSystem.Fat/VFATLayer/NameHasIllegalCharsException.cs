using System;

namespace Metran.FileSystem.Fat.VFATLayer
{
    /// <summary>
    /// The exception that is thrown when a name contains illegal characters
    /// </summary>
    public class NameHasIllegalCharsException : Exception
    {
        public NameHasIllegalCharsException(string message)
            : base(message)
        {
        }
    }
}