using System;

namespace Metran.FileSystem.Fat.VFATLayer
{
    /// <summary>
    /// The exception that is thrown when a short name cannot be generated due to an absence of free numeric tails
    /// </summary>
    public class NameCollisionException : Exception
    {
        public NameCollisionException(string message)
            : base(message)
        {
        }
    }
}