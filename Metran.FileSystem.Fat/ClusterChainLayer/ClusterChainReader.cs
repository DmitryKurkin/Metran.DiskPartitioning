using Metran.FileSystem.Fat.ClusterLayer;
using System;

namespace Metran.FileSystem.Fat.ClusterChainLayer
{
    /// <summary>
    /// Reads data from a chain of clusters using a FAT and Data Region. Tracks the end of the chain and returns zero bytes
    /// </summary>
    public class ClusterChainReader : IClusterChainReader
    {
        private readonly IFileAllocationTable _fat;

        private readonly IDataRegion _dataRegion;

        private int _currentCluster;

        private bool _lastClusterReached;

        public ClusterChainReader(IFileAllocationTable fat, IDataRegion dataRegion, int firstCluster)
        {
            if (fat == null) throw new ArgumentNullException(nameof(fat));
            if (dataRegion == null) throw new ArgumentNullException(nameof(dataRegion));

            if (dataRegion.ClustersCount != fat.TotalDataClusters)
            {
                throw new ArgumentException(
                    $"The data region must have the same length as the FAT does excluding reserved clusters ({fat.TotalDataClusters}). The current data region length is {dataRegion.ClustersCount}",
                    nameof(dataRegion));
            }

            fat.Validate(firstCluster);

            _fat = fat;
            _dataRegion = dataRegion;

            // we will use the first cluster in the next call to the ReadNextCluster
            _currentCluster = firstCluster;
        }

        int IClusterChainReader.BytesPerCluster => _dataRegion.SectorsPerCluster*_dataRegion.BytesPerSector;

        byte[] IClusterChainReader.ReadNextCluster()
        {
            // if we reached the last cluster in the chain, return zero bytes
            var clusterData = new byte[0];

            // did we reach the last cluster in the chain?
            if (!_lastClusterReached)
            {
                // no, we didn't

                // read data from the current cluster of the chain
                clusterData = _dataRegion.ReadCluster(_fat.MakeZeroBased(_currentCluster));

                // is the current cluster the last one in the chain?
                if (!_fat.IsLastCluster(_currentCluster))
                {
                    // no, it is not

                    // go to the next cluster in the chain
                    _currentCluster = _fat.TraverseCluster(_currentCluster);
                }
                else
                {
                    // we have reached the last cluster in the chain
                    _lastClusterReached = true;
                }
            }

            return clusterData;
        }
    }
}