using System;

namespace Metran.FileSystem.Fat.VFATLayer
{
    /// <summary>
    /// The exception that is thrown when a container already has the same name as provided by the user
    /// </summary>
    public class NameAlreadyExistsException : Exception
    {
        public NameAlreadyExistsException(string message)
            : base(message)
        {
        }
    }
}