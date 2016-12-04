using System;

namespace Metran.FileSystem
{
    /// <summary>
    /// Specifies attributes of a file system entity
    /// </summary>
    [Flags]
    public enum FileSystemEntityAttributes
    {
        None = 0, //0x0,
        Normal = 1 << 0, //0x01,
        ReadOnly = 1 << 1, //0x02,
        Hidden = 1 << 2, //0x04,
        System = 1 << 3, //0x08,
        Archive = 1 << 4 //0x10
    }
}