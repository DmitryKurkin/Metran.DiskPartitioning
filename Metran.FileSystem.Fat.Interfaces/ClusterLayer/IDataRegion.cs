namespace Metran.FileSystem.Fat.ClusterLayer
{
    /// <summary>
    /// Represents an array of clusters (i.e. contiguous groups of sectors).
    /// Provides read and write access to individual clusters by their numbers
    /// </summary>
    public interface IDataRegion
    {
        int ClustersCount { get; }

        int SectorsPerCluster { get; }

        int BytesPerSector { get; }

        byte[] ReadCluster(int cluster);

        void WriteCluster(int cluster, byte[] clusterData);
    }
}