using System.IO;

namespace Metran.FileSystem.Fat.ClusterChainStreamLayer
{
    /// <summary>
    /// Simplifies access to cluster chains allowing clients to work with them as with sequences of bytes
    /// </summary>
    public interface IClusterChainStreamManager
    {
        Stream CreateStream(out int firstCluster, bool resetAllocatedClustersData);

        Stream OpenStreamForWriting(int firstCluster, bool resetAllocatedClustersData);

        Stream OpenStreamForReading(int firstCluster);

        void DeleteStream(int firstCluster);
    }
}