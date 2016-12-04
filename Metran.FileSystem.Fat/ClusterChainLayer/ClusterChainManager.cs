using Metran.FileSystem.Fat.ClusterLayer;
using System;

namespace Metran.FileSystem.Fat.ClusterChainLayer
{
    public class ClusterChainManager : IClusterChainManager
    {
        private readonly IFileAllocationTable _fat;

        private readonly IDataRegion _dataRegion;

        public ClusterChainManager(IFileAllocationTable fat, IDataRegion dataRegion)
        {
            if (fat == null) throw new ArgumentNullException(nameof(fat));
            if (dataRegion == null) throw new ArgumentNullException(nameof(dataRegion));

            if (dataRegion.ClustersCount != fat.TotalDataClusters)
            {
                throw new ArgumentException(
                    $"The data region must have the same length as the FAT does excluding reserved clusters ({fat.TotalDataClusters}). The current data region length is {dataRegion.ClustersCount}",
                    nameof(dataRegion));
            }

            _fat = fat;
            _dataRegion = dataRegion;
        }

        IClusterChainWriter IClusterChainManager.CreateChain(out int firstCluster, bool resetAllocatedClustersData)
        {
            // allocate a cluster right away...
            IClusterChainWriter clusterChainWriter = new ClusterChainWriter(_fat, _dataRegion,
                resetAllocatedClustersData);

            // ...and save it
            firstCluster = clusterChainWriter.FirstCluster;

            return clusterChainWriter;
        }

        IClusterChainWriter IClusterChainManager.OpenChainForWriting(int firstCluster, bool resetAllocatedClustersData)
        {
            return new ClusterChainOverwriter(_fat, _dataRegion, resetAllocatedClustersData, firstCluster);
        }

        IClusterChainReader IClusterChainManager.OpenChainForReading(int firstCluster)
        {
            return new ClusterChainReader(_fat, _dataRegion, firstCluster);
        }

        void IClusterChainManager.DeleteChain(int firstCluster)
        {
            _fat.DeallocateClusterChain(firstCluster);
        }
    }
}