namespace Metran.FileSystem.Fat.ClusterChainLayer
{
    /// <summary>
    /// Generalizes the way of reading data from a chain of clusters. Returns zero bytes if the end of the chain has been reached
    /// </summary>
    public interface IClusterChainReader
    {
        int BytesPerCluster { get; }

        byte[] ReadNextCluster();
    }
}