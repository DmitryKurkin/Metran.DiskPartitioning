namespace Metran.FileSystem.Fat.ClusterChainLayer
{
    /// <summary>
    /// Generalizes the way of writing data to a chain of clusters
    /// </summary>
    public interface IClusterChainWriter
    {
        int BytesPerCluster { get; }

        int FirstCluster { get; }

        void WriteNextCluster(byte[] clusterData);

        void Close();
    }
}