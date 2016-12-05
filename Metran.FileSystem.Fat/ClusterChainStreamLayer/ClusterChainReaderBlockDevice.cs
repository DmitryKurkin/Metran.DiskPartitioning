using Metran.FileSystem.Fat.ClusterChainLayer;
using Metran.IO.Streams;
using System;

namespace Metran.FileSystem.Fat.ClusterChainStreamLayer
{
    /// <summary>
    /// Wraps a cluster chain reader into a block reader
    /// </summary>
    public class ClusterChainReaderBlockDevice : IBlockDevice
    {
        private readonly IClusterChainReader _clusterChainReader;

        public ClusterChainReaderBlockDevice(IClusterChainReader clusterChainReader)
        {
            if (clusterChainReader == null) throw new ArgumentNullException(nameof(clusterChainReader));

            _clusterChainReader = clusterChainReader;
        }

        int IBlockDevice.BlockSize => _clusterChainReader.BytesPerCluster;

        bool IBlockDevice.SupportsPositioning => false;

        bool IBlockDevice.SupportsReading => true;

        bool IBlockDevice.SupportsWriting => false;

        byte[] IBlockDevice.ReadBlock(int numberOfBlocks)
        {
            if (numberOfBlocks != 1)
            {
                throw new NotSupportedException("Reading a single cluster is only supported");
            }

            return _clusterChainReader.ReadNextCluster();
        }

        void IBlockDevice.WriteBlock(byte[] blockData)
        {
            throw new NotSupportedException();
        }

        long IBlockDevice.Position(long value)
        {
            throw new NotSupportedException();
        }

        void IBlockDevice.Close()
        {
            // nothing to do
        }
    }
}