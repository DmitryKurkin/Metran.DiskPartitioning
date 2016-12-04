using Metran.FileSystem.Fat.ClusterChainLayer;
using Metran.IO.Streams;
using System;
using System.IO;

namespace Metran.FileSystem.Fat.ClusterChainStreamLayer
{
    /// <summary>
    /// Uses buffered streams to implement IClusterChainStreamManager
    /// </summary>
    public class ClusterChainStreamManager : IClusterChainStreamManager
    {
        private readonly IClusterChainManager _clusterChainManager;

        public ClusterChainStreamManager(IClusterChainManager clusterChainManager)
        {
            if (clusterChainManager == null) throw new ArgumentNullException(nameof(clusterChainManager));

            _clusterChainManager = clusterChainManager;
        }

        Stream IClusterChainStreamManager.CreateStream(out int firstCluster, bool resetAllocatedClustersData)
        {
            // create a cluster chain
            var chainWriter = _clusterChainManager.CreateChain(out firstCluster, resetAllocatedClustersData);

            // use a buffered stream to write single bytes to the cluster chain
            var inputStream = new InputOutputBufferedStream(
                new ClusterChainWriterBlockDevice(chainWriter),
                new ByteListPipeBuffer());

            return inputStream;
        }

        Stream IClusterChainStreamManager.OpenStreamForWriting(int firstCluster, bool resetAllocatedClustersData)
        {
            // open a cluster chain
            var chainWriter = _clusterChainManager.OpenChainForWriting(firstCluster, resetAllocatedClustersData);

            // use a buffered stream to write single bytes to the cluster chain
            var inputStream = new InputOutputBufferedStream(
                new ClusterChainWriterBlockDevice(chainWriter),
                new ByteListPipeBuffer());

            return inputStream;
        }

        Stream IClusterChainStreamManager.OpenStreamForReading(int firstCluster)
        {
            // open a cluster chain
            var chainReader = _clusterChainManager.OpenChainForReading(firstCluster);

            // use a buffered stream to read single bytes from the cluster chain
            var outputStream = new InputOutputBufferedStream(
                new ClusterChainReaderBlockDevice(chainReader),
                new ByteListPipeBuffer());

            return outputStream;
        }

        void IClusterChainStreamManager.DeleteStream(int firstCluster)
        {
            _clusterChainManager.DeleteChain(firstCluster);
        }
    }
}