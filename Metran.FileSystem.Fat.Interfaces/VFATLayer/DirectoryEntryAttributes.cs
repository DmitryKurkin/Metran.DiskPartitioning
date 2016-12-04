using System;

namespace Metran.FileSystem.Fat.VFATLayer
{
    /// <summary>
    /// Specifies attributes of a directory entry
    /// </summary>
    [Flags]
    public enum DirectoryEntryAttributes : byte
    {
        None = 0, //0x0,
        ReadOnly = 1 << 0, //0x01,
        Hidden = 1 << 1, //0x02,
        System = 1 << 2, //0x04,
        VolumeLabel = 1 << 3, //0x08,
        Directory = 1 << 4, //0x10,
        Archive = 1 << 5, //0x20,
        Reserved1 = 1 << 6, //0x40,
        Reserved2 = 1 << 7, //0x80,
        LongName = ReadOnly | Hidden | System | VolumeLabel
    }
}