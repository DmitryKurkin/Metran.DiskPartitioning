namespace Metran.FileSystem.Fat.ClusterChainLayer
{
    /// <summary>
    /// Provides a simplified way of working with chains of clusters. Hides the presence of FAT and Data Region
    /// </summary>
    public interface IClusterChainManager
    {
        IClusterChainWriter CreateChain(out int firstCluster, bool resetAllocatedClustersData);

        IClusterChainWriter OpenChainForWriting(int firstCluster, bool resetAllocatedClustersData);

        IClusterChainReader OpenChainForReading(int firstCluster);

        void DeleteChain(int firstCluster);
    }
}