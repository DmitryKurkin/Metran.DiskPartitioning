using System;

namespace Metran.FileSystem.Fat.VFATLayer
{
    /// <summary>
    /// The exception that is thrown when a name has the length that is larger than the maximum allowable value
    /// </summary>
    public class NameTooLongException : Exception
    {
        public NameTooLongException(string message)
            : base(message)
        {
        }
    }
}