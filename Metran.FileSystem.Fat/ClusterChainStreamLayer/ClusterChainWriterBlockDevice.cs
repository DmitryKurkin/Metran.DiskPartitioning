using Metran.FileSystem.Fat.ClusterChainLayer;
using Metran.IO.Streams;
using System;

namespace Metran.FileSystem.Fat.ClusterChainStreamLayer
{
    /// <summary>
    /// Wraps a cluster chain writer into a block writer
    /// </summary>
    public class ClusterChainWriterBlockDevice : IBlockDevice
    {
        private readonly IClusterChainWriter _clusterChainWriter;

        public ClusterChainWriterBlockDevice(IClusterChainWriter clusterChainWriter)
        {
            if (clusterChainWriter == null) throw new ArgumentNullException(nameof(clusterChainWriter));

            _clusterChainWriter = clusterChainWriter;
        }

        int IBlockDevice.BlockSize => _clusterChainWriter.BytesPerCluster;

        bool IBlockDevice.SupportsPositioning => false;

        bool IBlockDevice.SupportsReading => false;

        bool IBlockDevice.SupportsWriting => true;

        byte[] IBlockDevice.ReadBlock()
        {
            throw new NotSupportedException();
        }

        void IBlockDevice.WriteBlock(byte[] blockData)
        {
            _clusterChainWriter.WriteNextCluster(blockData);
        }

        long IBlockDevice.Position(long value)
        {
            throw new NotSupportedException();
        }

        void IBlockDevice.Close()
        {
            _clusterChainWriter.Close();
        }
    }
}