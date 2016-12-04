namespace Metran.FileSystem.Fat.FileSystemLayer
{
    /// <summary>
    /// Specifies a set of volume parameters that are computed dynamically
    /// </summary>
    public struct VolumeDynamicInfo
    {
        public ushort RootDirectoryEntriesCount;

        public ushort ReservedSectorsCount;

        public byte FatsCount;

        public byte SectorsPerCluster;

        public uint FatSectorsCount;
    }
}