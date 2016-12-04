namespace Metran.FileSystem.Fat.FileSystemLayer
{
    /// <summary>
    /// Helps to map the size of a volume in sectors onto the number of sectors per cluster for the volume
    /// </summary>
    public struct VolumeSizeToSectorsPerCluster
    {
        public uint VolumeSectorsCount;

        public byte SectorsPerCluster;

        public VolumeSizeToSectorsPerCluster(uint volumeSectorsCount, byte sectorsPerCluster)
        {
            VolumeSectorsCount = volumeSectorsCount;
            SectorsPerCluster = sectorsPerCluster;
        }
    }
}