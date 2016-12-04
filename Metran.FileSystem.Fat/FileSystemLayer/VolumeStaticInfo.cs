namespace Metran.FileSystem.Fat.FileSystemLayer
{
    /// <summary>
    /// Specifies a set of static volume parameters
    /// </summary>
    public struct VolumeStaticInfo
    {
        public ushort BytesPerSector;

        public ushort SectorsPerTrack;

        public ushort TracksPerCylinder;

        public uint StartSector;

        public uint SectorsCount;
    }
}